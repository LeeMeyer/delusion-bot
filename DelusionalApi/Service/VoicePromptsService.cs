using Catalyst;
using DelusionalApi.Model;
using Flurl;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Http;
using Twilio.TwiML;
using static Twilio.TwiML.Voice.Gather;

namespace DelusionalApi.Service
{
    public class VoicePromptsService : IVoicePromptsService
    {
        private ISpeechService _speechService;
        private IHttpContextAccessor _httpContextAccessor;

        public VoicePromptsService(ISpeechService speechService, IHttpContextAccessor httpContextAccessor)
        {
            _speechService = speechService;
            _httpContextAccessor = httpContextAccessor;
        }

        public VoiceResponse FirstVoicePrompt(Uri callback)
        {
            var randomNoun = GetRandomNoun();

            var response = new VoiceResponse().Play(_speechService.VoiceUrl(
                string.Format("It's me. Mummy and I like to play a special game. You and I should play it, to help us understand each other. I say a word, then you say the first word that comes into your head. Got it? What do you think when I say. {0}?", randomNoun),
                Voice.Ren));

            return Gather(response, callback);
        }

        public VoiceResponse HandleVoicePromptResponse(string userReponse, int promptIndex)
        {
            if (promptIndex < 2)
            {
                var fun = RandomSynonym("fun");
                var listen = RandomSynonym("listen");
                var think = RandomSynonym("think");

                var response = new VoiceResponse();

                var delusionUrl = _httpContextAccessor.HttpContext.Request.WithPath("/Delusion").SetQueryParam("word", userReponse).ToUri();

                response = response.Play(delusionUrl);

                var randomNoun = GetRandomNoun();

                string comment = "See? It's " + fun + "! Now " + listen + ". What do you " + think + " when I say " + randomNoun + "?";

                response = response.Play(_speechService.VoiceUrl(comment, Voice.Ren));

                var callback = _httpContextAccessor.HttpContext.Request.WithPath("/HandleVoicePromptResponse").SetQueryParams(new { promptIndex = promptIndex + 1 }).ToUri();
                return Gather(response, callback);
            }
            else
            {
                Uri farewellUri = _speechService.VoiceUrl("Jesus mate. It's just a game, but the way you think is {0} and I'm hanging up.", Voice.Ren, RandomSynonym("disturbing"));
                return new VoiceResponse().Play(farewellUri);
            }
        }

        private static string RandomSynonym(string word)
        {
            var words = new List<string>();

            if (word == "disturbing")
            {
                words = new List<string> { "disturbing", "skewed", "evil", "messed up", "sick", "screwed up", "fucked", "disgusting", "stupid", "crazy", "insane", "wonky" };
            }
            else if (word == "fun")
            {
                words = new List<string> { "fun", "interesting", "amusing", "entertaining", "cool", "engaging", "illuminating", "funky" };
            }
            else if (word == "listen")
            {
                words = new List<string> { "listen", "pay attention", "focus",  "pay attention", "get a load of this", "focus", "check this out", "concentrate" };
            }
            else if (word == "think")
            {
                words = new List<string> { "think", "imagine", "want to say", "respond with" };
            }

            return words.OrderBy(w => Guid.NewGuid()).First();
        }

        private static string GetRandomNoun()
        {
            return WordNet.Nouns.GetAll().OrderBy(w => Guid.NewGuid()).First(w => w.Word.Length > 3 && w.PartOfSpeech == PartOfSpeech.NOUN && w.Word.Length < 5 && Char.IsLower(w.Word.First())).Word;
        }

        private VoiceResponse Gather(VoiceResponse response, Uri callback)
        {
            return response.Gather(action: callback,
                            input: new List<InputEnum> { InputEnum.Speech },
                            method: HttpMethod.Get,
                            profanityFilter: false,
                            language: LanguageEnum.EnAu,
                            enhanced: true);
        }
    }
}