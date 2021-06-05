using System;
using System.IO;
using System.Threading.Tasks;
using DelusionalApi.Model;
using DelusionalApi.Service;
using Microsoft.AspNetCore.Mvc;
using Twilio;

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
        private readonly IMadlibsService _madlibsService;

        public DelusionController(IConceptGraphDb conceptGraphDb, IAssociationFormatter associationFormatter,
            IDelusionDictionary delusionDictionary, ISpeechService speechService, AppSetttings appSetttings, 
            IMadlibsService madlibsService)
        {
            _conceptGraphDb = conceptGraphDb;
            _associationFormatter = associationFormatter;
            _delusionDictionary = delusionDictionary;
            _speechService = speechService;
            _appSetttings = appSetttings;
            _madlibsService = madlibsService;
        }

        [HttpPost]
        [Route("Call")]
        public async Task<IActionResult> Call(string phoneNumber)
        {
            if (phoneNumber.StartsWith("04"))
            {
                phoneNumber = "+61" + phoneNumber.Substring(1);
            }

            phoneNumber = phoneNumber.Replace(" ", "");

            TwilioClient.Init(_appSetttings.TwilioSettings.TwilioAccountSid, _appSetttings.TwilioSettings.TwilioAuthToken);

            var introduction = _madlibsService.InitializeGame(new Uri("/Delusion/HandleYesOrNo"), phoneNumber);

            /*var call = CallResource.Create(
                twiml: moo.ToString(),
                to: new Twilio.Types.PhoneNumber(phoneNumber),
                from: new Twilio.Types.PhoneNumber(_appSetttings.TwilioSettings.CallerId)
            );*/

            return new ContentResult
            {
                Content = introduction.ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        [HttpGet]
        [Route("HandleYesOrNo")]

        public async Task<IActionResult> HandleYesOrNo(string SpeechResult, string phoneNumber)
        {
            var response = await _madlibsService.HandleYesOrNo(SpeechResult, phoneNumber, new Uri("/Delusion/HandleTokenPrompt"));
           
            return new ContentResult
            {
                Content = response.ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }


        [HttpGet]
        [Route("HandleTokenPrompt")]
        public IActionResult HandleTokenPrompt(string SpeechResult, string phoneNumber, int promptIndex)
        {
            var response = _madlibsService.HandleTokenPrompt(SpeechResult, phoneNumber, promptIndex);

            return new ContentResult
            {
                Content = response.ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        [HttpGet]
        [Route("Say")]
        [ApiExplorerSettings(IgnoreApi = true)]

        public async Task<IActionResult> Say(Voice voice, string words)
        {
            var audioStream = await _speechService.SpeakText(words, voice);

            var filename = Guid.NewGuid() + ".wav";

            var filePath = Path.Combine(System.AppContext.BaseDirectory, filename);
            await audioStream.SaveToWaveFileAsync(filePath);
            var bytes = System.IO.File.ReadAllBytes(filePath);
            System.IO.File.Delete(filePath); 

            return File(bytes, "audio/wav", $"deulsion.wav");
        }


        /// <summary>
        /// Gets delusional text connecting the passed-in word to a delusion. Even when the parameters are the same,
        /// response text generation involves randomness and can vary for each request.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /Delusion?word=swim&amp;delusionType=Intruders
        ///     
        /// Sample response:
        /// 
        ///     "Heat causes the desire to swim. Starting flame or fire requires heat. Starting flame or fire causes death. Death is created by homicide. Manslaughter is homicide.  I killed the invader in the bathtub. I grabbed him, and ran to the tub, strangling and suffocating him with the wires under cold water."
        ///
        /// Sample request:
        ///
        ///     GET /Delusion?word=swim&amp;delusionType=Impregnate
        ///     
        /// Sample response:
        /// 
        ///     "Fish can swim. Fish is animal. Animal can live. Live requires conceived.  You know you were sired via illegal artificial insemination."
        /// </remarks>
        /// <param name="word"></param>
        /// <param name="delusionType">If not specified, defaults to random</param>
        /// <returns>Delusional ramblings</returns>
        /// <response code="200">Returns a delusional string</response>           
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Get(Voice voice, string word = "silence", DelusionType? delusionType = null)
        {
            if (!delusionType.HasValue)
            {
                delusionType = (DelusionType)new Random().Next(0, (int)DelusionType.Impregnate);
            }

            var endConcept = _delusionDictionary.RandomConcept(delusionType.Value);
            var associations = _associationFormatter.Format(await _conceptGraphDb.GetAssociations(word, endConcept));
            var delusionDescription = _delusionDictionary.DescribeDelusion(delusionType.Value);
            var output = $"{associations} {delusionDescription}";

            var audioStream = await _speechService.SpeakText(output, voice);

            var filePath = Path.Combine(System.AppContext.BaseDirectory, "delusion.wav");
            await audioStream.SaveToWaveFileAsync(filePath);
            var bytes = System.IO.File.ReadAllBytes(filePath);

            return File(bytes, "audio/wav", $"deulsion.wav");
        }


    }
}