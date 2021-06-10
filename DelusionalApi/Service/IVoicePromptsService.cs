using System;
using Twilio.TwiML;

namespace DelusionalApi.Service
{
    public interface  IVoicePromptsService
    {
        VoiceResponse FirstVoicePrompt(Uri callback);
        VoiceResponse HandleVoicePromptResponse(string userReponse, int promptIndex);
    }
}