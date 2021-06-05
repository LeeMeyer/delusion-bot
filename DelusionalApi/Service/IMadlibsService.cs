using DelusionalApi.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio.TwiML;

namespace DelusionalApi.Service
{
    public interface  IMadlibsService
    {
        Task<VoiceResponse> InitializeGame(Uri callback, string phoneNumber);
        Task<VoiceResponse> HandleYesOrNo(string userReponse, string phoneNumber, Uri yesCallback);
        Task<VoiceResponse> HandleTokenPrompt(string userReponse, string phoneNumber, int promptIndex);
    }
}