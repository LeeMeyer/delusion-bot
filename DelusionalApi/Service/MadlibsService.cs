using Catalyst;
using DelusionalApi.Data;
using DelusionalApi.Model;
using Mosaik.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Twilio.Http;
using Twilio.TwiML;
using static Twilio.TwiML.Voice.Gather;

namespace DelusionalApi.Service
{
    public class MadlibsService : IMadlibsService
    {
        private ISpeechService _speechService;
        private IConceptGraphDb _conceptGraphDb;

        public MadlibsService(ISpeechService speechService, IConceptGraphDb conceptGraphDb)
        {
            _speechService = speechService;
            _conceptGraphDb = conceptGraphDb;
        }

        public async Task<List<TokenPrompt>> GetTokenPrompts(int startIndex)
        {
            Document doc = await GetExcerptDocument(startIndex);

            List<TokenPrompt> tokenPrompts = GetTokenPrompts(doc);

            return tokenPrompts;
        }

        private static List<TokenPrompt> GetTokenPrompts(Document doc)
        {
            var tokens = doc.ToTokenList();

            var wordsICareAbout = tokens.Where(t => t.POS == PartOfSpeech.ADJ || t.POS == PartOfSpeech.NOUN || t.POS == PartOfSpeech.VERB).ToList();

            var tokenPrompts = wordsICareAbout
                .Skip(3)
                .ToList()
                .Select(token => new TokenPrompt { Token = token, NeighbouringToken = wordsICareAbout[token.Index - 2] })
                .OrderBy(o => Guid.NewGuid())
                .Take(5)
                .ToList();
            return tokenPrompts;
        }

        private static async Task<Document> GetExcerptDocument(int startIndex)
        {
            string sampledSentences = GetExcerpt(startIndex);

            var doc = new Document(sampledSentences, Language.English);

            Storage.Current = new OnlineRepositoryStorage(new DiskStorage("catalyst-models"));
            var nlp = await Pipeline.ForAsync(Language.English);

            nlp.ProcessSingle(doc);
            return doc;
        }

        private static string GetExcerpt(int startIndex)
        {
            return string.Join(". ", Excerpt.MemoirsSentences.Skip(startIndex).Take(3).ToArray());
        }

        public async Task<VoiceResponse> InitializeGame(Uri callback, string phoneNumber)
        {
            var response = new VoiceResponse().Play(_speechService.VoiceUrl("Hey! I'm writing a poem. Can you help?"));

            var startIndex = new Random().Next(0, Excerpt.MemoirsSentences.Length - 4);
            var tokenPrompts = await GetTokenPrompts(startIndex);

            await _conceptGraphDb.SavePoem(new PhonePoem
            {
                ExcerptIndex = startIndex,
                PhoneNumber = phoneNumber,
                TokenIndexes = tokenPrompts.Select(t => t.Token.Index).ToList()
            });

            return Gather(response, callback.WithQuery("phoneNumber", phoneNumber));
        }

        public async Task<VoiceResponse> HandleYesOrNo(string userReponse, string phoneNumber, Uri yesCallback)
        {
            var result = new VoiceResponse();

            if (userReponse.Contains("n", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Play(_speechService.VoiceUrl("Ok! Whatever! Bye!"));
            }
            else
            {
                result.Play(_speechService.VoiceUrl("Thank you very much! I Love you!"));
                result.Play(await TokenPromptUri(phoneNumber, 0));
                Gather(result, yesCallback.WithQuery("phoneNumber", phoneNumber).WithQuery("promptIndex", 1));
            }

            return result;
        }

        public async Task<VoiceResponse> HandleTokenPrompt(string userReponse, string phoneNumber, int promptIndex)
        {
            var phonePoem = await _conceptGraphDb.GetPoem(phoneNumber);
            phonePoem.ReplacementWords.Add(userReponse);
            await _conceptGraphDb.SavePoem(phonePoem);

            var result = new VoiceResponse();

            if (promptIndex < 5)
            {
                result.Play(await TokenPromptUri(phoneNumber, promptIndex));
                Gather(result, null);
            }
            else
            {
               result = await Say(phonePoem);
            }

            return result;
        }

        private async Task<VoiceResponse> Say(PhonePoem phonePoem)
        {
            var result = new VoiceResponse();
            Document doc = await GetExcerptDocument(phonePoem.ExcerptIndex);
            List<TokenPrompt> tokenPrompts = GetTokenPrompts(doc);

            for (int i = 0; i < tokenPrompts.Count(); i++)
            {
                tokenPrompts[i].Token.Replacement = phonePoem.ReplacementWords[i];
            }

            return result.Play(_speechService.VoiceUrl(doc.Value));
        }


        private async Task<Uri> TokenPromptUri(string phoneNumber, int promptIndex)
        {
            var phonePoem = await _conceptGraphDb.GetPoem(phoneNumber);
            var tokenPrompts = await GetTokenPrompts(phonePoem.ExcerptIndex);

            var tokenIndex = phonePoem.TokenIndexes[promptIndex];
            var tokenToReplace = tokenPrompts[tokenIndex];
            var wordCategory = string.Empty;

            switch (tokenToReplace.Token.POS)
            {
                case PartOfSpeech.ADJ:
                    wordCategory = "adjective";
                    break;
                case PartOfSpeech.NOUN:
                    wordCategory = "noun";
                    break;
                case PartOfSpeech.VERB:
                    wordCategory = "verb";
                    break;
            }

            return _speechService.VoiceUrl(
                "Give me {0} {1} that rhymes with {2}",
                tokenToReplace.Token.POS == PartOfSpeech.ADJ ? "an" : "a", wordCategory, tokenToReplace.NeighbouringToken.Value);
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
