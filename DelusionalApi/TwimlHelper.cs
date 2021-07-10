using System;
using System.Collections.Generic;
using Twilio.Http;
using Twilio.TwiML;
using static Twilio.TwiML.Voice.Gather;

namespace DelusionalApi
{
    public static class TwimlHelper
    {
        public static VoiceResponse Gather(this VoiceResponse response, Uri callback, Uri nullUri)
        {
            return response.Gather(
                            action: nullUri,
                            partialResultCallback: callback,
                            partialResultCallbackMethod: HttpMethod.Get,
                            input: new List<InputEnum> { InputEnum.Speech },
                            method: HttpMethod.Get,
                            profanityFilter: false,
                            language: LanguageEnum.EnAu,
                            speechTimeout: "3600");
        }
    }
}