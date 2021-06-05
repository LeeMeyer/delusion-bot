using DelusionalApi.Model;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Threading.Tasks;
using System.Web;

namespace DelusionalApi.Service
{
    public class CustomVoiceService : ISpeechService
    {
        public async Task<AudioDataStream> SpeakText(string text, Voice voice)
        {
            string subscriptionKey = "1718577cfb134a6bb0c74ca969746a31";
            string subscriptionRegion = "eastus";

            var config = SpeechConfig.FromSubscription(subscriptionKey, subscriptionRegion);


            switch (voice)
            {
                case Voice.Ren:
                    config.EndpointId = "0bf69baf-8cb4-485e-843d-69811951ed29";
                    config.SpeechSynthesisVoiceName = "RenbotNeural";
                    break;
                case Voice.Felicity:
                    config.EndpointId = "4ccf2b79-a321-4937-853a-95bbd0a348f2";
                    config.SpeechSynthesisVoiceName = "fNeural";
                    break;
                case Voice.Phil:
                    config.EndpointId = "e0f691f7-9fd7-4726-9a4b-df93613a84ac";
                    config.SpeechSynthesisVoiceName = "PhilNeural";
                    break;
                case Voice.Bella:
                    config.EndpointId = "bc0c3a77-e62c-4f9f-af90-fc3d1644b148";
                    config.SpeechSynthesisVoiceName = "Bella Bot 3Neural";
                    break;

            }
 
            config.SetProfanity(ProfanityOption.Raw);
            
            using (var synthesizer = new SpeechSynthesizer(config, null))
            {
                using (var result = await synthesizer.SpeakTextAsync(text))
                {
                    if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        return AudioDataStream.FromResult(result);
                    }
                }
            }

            return null;
        }

        public Uri VoiceUrl(string words, params string[] args)
        {
            return "/Delusion/Say"
                .WithQuery("words", string.Format(words, args))
                .WithQuery("voice", Voice.Bella.ToString());
        }
    }
}