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
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

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
        private readonly ILogger<DelusionController> _logger;

        public DelusionController(IConceptGraphDb conceptGraphDb, IAssociationFormatter associationFormatter,
            IDelusionDictionary delusionDictionary, ISpeechService speechService, AppSetttings appSetttings,
            BotScriptService botScriptService, IMemoryCache memoryCache, ILogger<DelusionController> logger)
        {
            _conceptGraphDb = conceptGraphDb;
            _associationFormatter = associationFormatter;
            _delusionDictionary = delusionDictionary;
            _speechService = speechService;
            _appSetttings = appSetttings;
            _botScriptService = botScriptService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        [HttpPost]
        [Route("Call")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IActionResult Call(string phoneNumber)
        {
            var voice = Voice.Ren;
            _botScriptService.GenerateUsedDirectory(voice);
            
            for (int i = 0; i <= 2; i++)
            {
                var key = GetPriorInputCacheKey(i, voice);
                _memoryCache.Remove(key);
            }

            var usedPath = _botScriptService.GetRelativeUsedPath(voice);

            if (phoneNumber.StartsWith("04"))
            {
                phoneNumber = "+61" + phoneNumber.Substring(1);
            }

            phoneNumber = phoneNumber.Replace(" ", "");

            TwilioClient.Init(_appSetttings.TwilioSettings.TwilioAccountSid, _appSetttings.TwilioSettings.TwilioAuthToken);

            var twiml = new VoiceResponse()
                //.Play(HttpContext.Request.WithPath(usedPath, "intro.wav"))
                .Play(HttpContext.Request.WithPath(usedPath, "prompt_0.wav"))
                .Gather(HttpContext.Request
                                .WithPath("Delusion/HandleVoicePromptResponse")
                                .SetQueryParams(new { promptIndex = 0, voice })
                                .ToUri(),
                        HttpContext.Request.WithPath("Delusion/Null"))
                .Pause(3600);

               CallResource.Create(
                   record: false,
                   twiml: twiml.ToString(),
                   to: new Twilio.Types.PhoneNumber(phoneNumber),
                   from: new Twilio.Types.PhoneNumber(_appSetttings.TwilioSettings.CallerId)
               );

            Console.WriteLine("testing");

            
            return new ContentResult
            {
                Content = twiml.ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        [HttpGet]
        [Route("HandleVoicePromptResponse")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> HandleVoicePromptResponse([FromQuery] string CallSid, [FromQuery] string UnstableSpeechResult, [FromQuery] int SequenceNumber, [FromQuery] decimal Stability, [FromQuery] int promptIndex, [FromQuery] Voice voice)
        {
            if (promptIndex >= 2)
            {
                var twiml = new VoiceResponse().Play(HttpContext.Request.WithPath(_botScriptService.GetRelativeUsedPath(voice), "goodbye.wav"));

                var callUpdated = false;

                while (!callUpdated)
                {
                    try
                    {
                        CallResource.Update(pathSid: CallSid, twiml: twiml.ToString());
                        callUpdated = true;
                    }
                    catch
                    {
                        //you always know you're in a good situation when you find yourself doing shit like this :(
                        await Task.Delay(1000);
                    }
                }
            }
            else
            {
                var key = GetPriorInputCacheKey(promptIndex, voice);
                if (!_memoryCache.Get<(int sequenceNumber, decimal stability, bool exists, bool delusionGenerated)>(key).exists)
                {
                    var currentInput = (sequenceNumber: SequenceNumber, stability: Stability, exists: true, delusionGenerated: false);
                    _memoryCache.Set(key, currentInput);

                    await UpdateCallResponse(CallSid, promptIndex, voice);
                    await GenerateDelusion(UnstableSpeechResult, SequenceNumber, Stability, promptIndex, voice);
                }
                else
                {
                    await GenerateDelusion(UnstableSpeechResult, SequenceNumber, Stability, promptIndex, voice);
                }
            }

            return new ContentResult
            {
                Content = string.Empty,
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        [HttpGet]
        [Route("Null")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Null()
        {
            return new ContentResult
            {
                Content = new VoiceResponse().Pause(10000).ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        private async Task UpdateCallResponse(string CallSid, int promptIndex, Voice voice)
        {
            var twiml =
                new VoiceResponse()
                        .Play(HttpContext.Request.WithPath(_botScriptService.GetRelativeUsedPath(voice), $"remark_{promptIndex}.wav"))
                        .Play(HttpContext.Request.WithPath(_botScriptService.GetRelativeUsedPath(voice), $"delusion_{promptIndex}.wav"))
                        .Play(HttpContext.Request.WithPath(_botScriptService.GetRelativeUsedPath(voice), $"prompt_{promptIndex + 1}.wav"))
                        .Gather(HttpContext.Request
                                .WithPath("Delusion/HandleVoicePromptResponse")
                                .SetQueryParams(new { promptIndex = promptIndex + 1, voice })
                                .ToUri(),
                                HttpContext.Request.WithPath("Delusion/Null"))
                .Pause(3600);


            var callUpdated = false;

            while (!callUpdated)
            {
                try
                {
                    CallResource.Update(pathSid: CallSid, twiml: twiml.ToString());
                    callUpdated = true;
                }
                catch
                {
                    //you always know you're in a good situation when you find yourself doing shit like this :(
                    await Task.Delay(1000);
                }
            }
        }

        private async Task GenerateDelusion(string UnstableSpeechResult, int SequenceNumber, decimal Stability, int promptIndex, Voice voice)
        {
            if (!string.IsNullOrWhiteSpace(UnstableSpeechResult))
            {
                UnstableSpeechResult = new string(UnstableSpeechResult.Split().First().Where(c => !char.IsPunctuation(c)).ToArray());

                var priorInputCacheKey = GetPriorInputCacheKey(promptIndex, voice);
                var priorInput = _memoryCache.Get<(int sequenceNumber, decimal stability, bool exists, bool delusionGenerated)>(priorInputCacheKey);

                _logger.LogInformation("thiking about delusions for {UnstableSpeechResult} {SequenceNumber} {Stability}", UnstableSpeechResult, SequenceNumber, Stability);

                if (!priorInput.delusionGenerated || (SequenceNumber >= priorInput.sequenceNumber && Stability >= priorInput.stability))
                {
                    _logger.LogInformation($"Passed cache check for {UnstableSpeechResult} {SequenceNumber} {Stability}!", UnstableSpeechResult, priorInput.sequenceNumber, priorInput.stability);

                    var currentInput = (sequenceNumber: SequenceNumber, stability: Stability, exists: true, delusionGenerated: true);
                    _memoryCache.Set(priorInputCacheKey, currentInput);

                    DelusionType[] delusions = ReadDelusionsScript(voice);

                    var delusionPath = Path.Combine(_botScriptService.GetFullUsedPath(voice), $"delusion_{promptIndex}.wav");

                    await SaveDelusion(voice, UnstableSpeechResult, delusions[promptIndex], delusionPath);

                    _logger.LogInformation("generated a delusion for {UnstableSpeechResult} {SequenceNumber} {Stability}!", UnstableSpeechResult, SequenceNumber, Stability);
                }
            }
        }

        private static string GetPriorInputCacheKey(int promptIndex, Voice voice)
        {
            return $"prior-input-{voice}-{promptIndex}";
        }

        private DelusionType[] ReadDelusionsScript(Voice voice)
        {
            var delusionScriptPath = Path.Combine(_botScriptService.GetFullUsedPath(voice), "delusions.json");
            JObject delusionTypes = JObject.Parse(System.IO.File.ReadAllText(delusionScriptPath));
            var delusions = ((JArray)delusionTypes["Delusions"]).Select(a => (DelusionType)a.Value<int>()).ToArray();
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