using System.IO;
using System.Threading.Tasks;
using DelusionalApi.Model;
using DelusionalApi.Service;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Flurl;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace DelusionalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DelusionController : ControllerBase
    {
        private readonly IConceptGraphDb _conceptGraphDb;
        private readonly IAssociationFormatter _associationFormatter;
        private readonly IDelusionDictionary _delusionDictionary;
        private readonly ISpeechService _speechService;
        private readonly AppSetttings _appSetttings;
        private readonly BotScriptService _botScriptService;
        private readonly IMemoryCache _memoryCache;

        public DelusionController(IConceptGraphDb conceptGraphDb, IAssociationFormatter associationFormatter,
            IDelusionDictionary delusionDictionary, ISpeechService speechService, AppSetttings appSetttings,
            BotScriptService botScriptService, IMemoryCache memoryCache)
        {
            _conceptGraphDb = conceptGraphDb;
            _associationFormatter = associationFormatter;
            _delusionDictionary = delusionDictionary;
            _speechService = speechService;
            _appSetttings = appSetttings;
            _botScriptService = botScriptService;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        [Route("Call")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IActionResult Call(string phoneNumber)
        {
            var voice = Voice.Ren;
            _botScriptService.GenerateUsedDirectory(voice);
            var usedPath = _botScriptService.GetRelativeUsedPath(voice);

            if (phoneNumber.StartsWith("04"))
            {
                phoneNumber = "+61" + phoneNumber.Substring(1);
            }

            phoneNumber = phoneNumber.Replace(" ", "");

            TwilioClient.Init(_appSetttings.TwilioSettings.TwilioAccountSid, _appSetttings.TwilioSettings.TwilioAuthToken);

            var twiml = new VoiceResponse()
                .Play(HttpContext.Request.WithPath(Path.Combine(usedPath, "intro.wav")))
                .Play(HttpContext.Request.WithPath(Path.Combine(usedPath, "prompt_0.wav")))
                .Gather(HttpContext.Request
                                .WithPath("HandleVoicePromptResponse")
                                .SetQueryParams(new { promptIndex = 0, voice })
                                .ToUri())
                .Pause(int.MaxValue);

            CallResource.Create(
                record: true,
                twiml: twiml.ToString(),
                to: new Twilio.Types.PhoneNumber(phoneNumber),
                from: new Twilio.Types.PhoneNumber(_appSetttings.TwilioSettings.CallerId)
            );

            return new ContentResult
            {
                Content = string.Empty,
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        [HttpGet]
        [Route("HandleVoicePromptResponse")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> HandleVoicePromptResponse([FromQuery] string CallSid, [FromQuery] string UnstableSpeechResult, [FromQuery] int SequenceNumber, [FromQuery] decimal Stability, [FromQuery] int promptIndex, [FromQuery] Voice voice)
        {
            var previousPromptFile = Path.Combine(_botScriptService.GetFullUsedPath(voice), $"prompt_{promptIndex}.wav");
           
            if (promptIndex >= 2)
            {
                var goodbyePath = Path.Combine(_botScriptService.GetRelativeUsedPath(voice), $"goodbye.wav");
                var twiml = new VoiceResponse().Play(HttpContext.Request.WithPath(goodbyePath));

                await Task.Delay(200);

                CallResource.Update(pathSid: CallSid, twiml: twiml.ToString());
            }
            else
            {
                if (System.IO.File.Exists(previousPromptFile))
                {
                    System.IO.File.Delete(previousPromptFile);

                    var commentPath = Path.Combine(_botScriptService.GetRelativeUsedPath(voice), $"remark_{promptIndex}.wav");
                    var delusionPath = Path.Combine(_botScriptService.GetRelativeUsedPath(voice), $"delusion_{promptIndex}.wav");

                    var twiml = 
                        new VoiceResponse()
                                .Play(HttpContext.Request.WithPath(commentPath))
                                .Play(HttpContext.Request.WithPath(delusionPath))
                                .Gather(HttpContext.Request.WithPath("HandleVoicePromptResponse").SetQueryParams(new { promptIndex, voice }).ToUri());

                    CallResource.Update(pathSid: CallSid, twiml: twiml.ToString());
                }

                await GenerateDelusion(UnstableSpeechResult, SequenceNumber, Stability, promptIndex, voice);
            }

            return Ok();
        }

        private async Task GenerateDelusion(string UnstableSpeechResult, int SequenceNumber, decimal Stability, int promptIndex, Voice voice)
        {
            var priorInputCacheKey = $"prior-input-{voice}-{promptIndex}";
            var priorInput = _memoryCache.Get<(int sequenceNumber, decimal stability, bool exists)>(priorInputCacheKey);

            if (!priorInput.exists || (SequenceNumber > priorInput.sequenceNumber && Stability >= priorInput.stability))
            {
                DelusionType[] delusions = ReadDelusionsScript(voice);

                var delusionPath = Path.Combine(_botScriptService.GetFullUsedPath(voice), $"delusion_{promptIndex}.wav");

                await SaveDelusion(voice, UnstableSpeechResult, delusions[promptIndex], delusionPath);

                var currentInput = (sequenceNumber: SequenceNumber, stability: Stability, exists: true);
                _memoryCache.Set(priorInputCacheKey, currentInput);
            }
        }

        private DelusionType[] ReadDelusionsScript(Voice voice)
        {
            var delusionScriptPath = Path.Combine(_botScriptService.GetFullUsedPath(voice), "delusions.json");
            JObject delusionTypes = JObject.Parse(System.IO.File.ReadAllText(delusionScriptPath));
            var delusions = delusionTypes["Delusions"].Value<DelusionType[]>();
            return delusions;
        }

        private async Task SaveDelusion(Voice voice, string word, DelusionType? delusionType, string filePath)
        {
            var endConcept = _delusionDictionary.RandomConcept(delusionType.Value);
            var associations = _associationFormatter.Format(await _conceptGraphDb.GetAssociations(word, endConcept));
            var delusionDescription = _delusionDictionary.DescribeDelusion(delusionType.Value);
            var output = $"{associations} {delusionDescription}";

            var audioStream = await _speechService.SpeakText(output, voice);

            await audioStream.SaveToWaveFileAsync(filePath);
        }
    }
}