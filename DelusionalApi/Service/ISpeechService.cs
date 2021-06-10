using DelusionalApi.Model;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;

namespace DelusionalApi.Service
{
    public interface ISpeechService
    {
        Task<AudioDataStream> SpeakText(string text, Voice voice);
        Uri VoiceUrl(string words, Voice voice, params string[] args);
    }
}