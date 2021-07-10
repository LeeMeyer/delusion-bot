using DelusionalApi.Model;
using DelusionalApi.Model.Bots;
using Microsoft.CognitiveServices.Speech;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DelusionalApi.Service
{
    public class BotScriptService
    {
        ISpeechService _speechService;

        public BotScriptService(ISpeechService speechService)
        {
            _speechService = speechService;
        }

        public async Task PrepareBotScripts<T>(T bot, int rounds) where T : IBot
        {
            var randomString = new RandomString();
            
            var botDirectory = Path.Combine(Environment.CurrentDirectory, "Scripts", bot.Voice.ToString());
            Directory.CreateDirectory(botDirectory);

            var lockfilePath = Path.Combine(botDirectory, ".lock");

            if (!File.Exists(lockfilePath))
            {
                File.Create(Path.Combine(botDirectory, ".lock"));

                await Save(bot.Intro(randomString), $"{botDirectory}/intro.wav", bot.Voice);
                await Save(bot.Goodbye(randomString), $"{botDirectory}/goodbye.wav", bot.Voice);

                using (var streamWriter = new StreamWriter($"{botDirectory}/delusions.json"))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName($"Delusions");

                    jsonWriter.WriteValue(bot.Delusions.OrderBy(d => Guid.NewGuid()).Take(rounds).ToArray());                    

                    jsonWriter.WriteEndObject();
                    streamWriter.Flush();
                }

                var remarks = new List<AudioDataStream>();
                var prompts = new List<AudioDataStream>();

                for (int i = 0; i < rounds; i++)
                {
                    remarks.Add(await _speechService.SpeakText(bot.InterstitialRemark(i, randomString), bot.Voice));
                    prompts.Add(await _speechService.SpeakText(bot.Prompt(i, randomString), bot.Voice));
                }

                for (int i = 0; i < remarks.Count(); i++)
                {
                    await remarks[i].SaveToWaveFileAsync($"{botDirectory}/remark_{i}.wav");
                }

                for (int i = 0; i < prompts.Count(); i++)
                {
                    await prompts[i].SaveToWaveFileAsync($"{botDirectory}/prompt_{i}.wav");
                }
            }
        }


        public void GenerateUsedDirectory(Voice voice)
        {
            var directoryPath = Path.Combine(Environment.CurrentDirectory, "Scripts", voice.ToString());
            var usedFolder = GetFullUsedPath(voice);
            Directory.CreateDirectory(usedFolder);
            Directory.Delete(usedFolder, true);
            FileSystem.CopyDirectory(directoryPath, usedFolder);
            FileSystem.DeleteFile(Path.Combine(directoryPath, ".lock"));
        }

        public string GetFullUsedPath(Voice voice)
        {
            var relativeUsedPath = GetRelativeUsedPath(voice);
            var usedFolder = Path.Combine(Environment.CurrentDirectory, relativeUsedPath);
            return usedFolder;
        }

        public string GetRelativeUsedPath(Voice voice)
        {
            return Path.Combine("Scripts", $"{voice}_Used");
        }

        private async Task Save(string s, string path, Voice voice)
        {
            var audioStream = await _speechService.SpeakText(s, voice);
            await audioStream.SaveToWaveFileAsync(path);
        }

    }
}
