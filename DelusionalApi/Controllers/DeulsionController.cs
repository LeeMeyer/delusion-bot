using System.IO;
using System.IO;
using System.Threading.Tasks;
using DelusionalApi.Model;
using DelusionalApi.Service;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.TwiML;
using Flurl;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Hangfire;
using Microsoft.CognitiveServices.Speech;

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
        private readonly CallService _callService;

        private readonly Dictionary<Voice, BotSettings> _botSettings = new Dictionary<Voice, BotSettings>
        {
            { 
                Voice.Ren, 
                new BotSettings 
                { 

                    Phone = "0416024337", 
                    CallDelayInSeconds = 570,
                    OscMessages = new List<Osc>
                    {
                        new Osc
                        {
                            Address = "/composition/columns/6/connect",
                            Port = 9000
                        },
                        new Osc
                        {
                            Address = "/unity",
                            Argument = 5,
                            Port = 9001
                        }
                    }
                }
            },
            {
                Voice.Felicity,
                new BotSettings
                {
                    Phone = "0416024337",
                    CallDelayInSeconds = 590,
                    OscMessages = new List<Osc>
                    {
                        new Osc
                        {
                            Address = "/composition/columns/5/connect",
                            Port = 9000
                        },
                        new Osc
                        {
                            Address = "/unity",
                            Argument = 4,
                            Port = 9001
                        }
                    }

                }
            },
            {
                Voice.Phil,
                new BotSettings
                {
                    Phone = "0416024337",
                    CallDelayInSeconds = 560,
                    OscMessages = new List<Osc>
                    {
                        new Osc
                        {
                            Address = "/composition/columns/4/connect",
                            Port = 9000
                        },
                        new Osc
                        {
                            Address = "/unity",
                            Argument = 3,
                            Port = 9001
                        }
                    }
                }
            },
            {
                Voice.Bella,
                new BotSettings
                {
                    Phone = "0416024337",
                    CallDelayInSeconds = 585,
                    OscMessages = new List<Osc>
                    {
                        new Osc {
                            Address = "/composition/columns/3/connect",
                            Port = 9000
                        },
                        new Osc
                        {   
                            Address = "/unity",
                            Argument = 2,
                            Port = 9001
                        }

                    }

                }
            }
        };

        public DelusionController(IConceptGraphDb conceptGraphDb, IAssociationFormatter associationFormatter,
            IDelusionDictionary delusionDictionary, ISpeechService speechService, AppSetttings appSetttings,
            BotScriptService botScriptService, IMemoryCache memoryCache, ILogger<DelusionController> logger, CallService callService)
        {
            _conceptGraphDb = conceptGraphDb;
            _associationFormatter = associationFormatter;
            _delusionDictionary = delusionDictionary;
            _speechService = speechService;
            _appSetttings = appSetttings;
            _botScriptService = botScriptService;
            _memoryCache = memoryCache;
            _logger = logger;
            _callService = callService;
        }

        [HttpPost]
        [Route("Call")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IActionResult Call()
        {
            foreach (var voice in _botSettings.Keys.Where(k => k == Voice.Bella))
            {
                _botScriptService.GenerateUsedDirectory(voice);
                
                var usedPath = _botScriptService.GetRelativeUsedPath(voice);

                var settings = _botSettings[voice];
                var phoneNumber = settings.Phone;

                if (phoneNumber.StartsWith("04"))
                {
                    phoneNumber = "+61" + phoneNumber.Substring(1);
                }

                phoneNumber = phoneNumber.Replace(" ", "");

                TwilioClient.Init(_appSetttings.TwilioSettings.TwilioAccountSid,
                    _appSetttings.TwilioSettings.TwilioAuthToken);

                var twiml = new VoiceResponse()
                    .Play(HttpContext.Request.WithPath(usedPath, "intro.wav"))
                    .Play(HttpContext.Request.WithPath(usedPath, "prompt_0.wav"))
                    .Gather(HttpContext.Request
                            .WithPath("Delusion/HandleVoicePromptResponse")
                            .SetQueryParams(new {promptIndex = 0, voice})
                            .ToUri(),
                        HttpContext.Request.WithPath("Delusion/EndGather")
                            .SetQueryParams(new {promptIndex = 0, voice})
                            .ToUri());

                var twimlString = twiml.ToString();

                var callCompletedCallback = HttpContext.Request
                    .WithPath("Delusion/CallStatus")
                    .SetQueryParams(new {voice})
                    .ToUri();

                BackgroundJob.Schedule(
                    () => _callService.ScheduleCall(phoneNumber, twimlString, callCompletedCallback),
                    TimeSpan.FromSeconds(_botSettings[voice].CallDelayInSeconds));
            }

            return Ok();
        }


        [HttpPost]
        [Route("CallStatus")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> CallStatus([FromQuery] Voice voice)
        {
            foreach (var oscMessage in _botSettings[voice].OscMessages)
            {
                await PostOsc(oscMessage);
            }
            
            return new ContentResult
            {
                Content = new VoiceResponse().ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        private async Task PostOsc(Osc command)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://stargazed.ngrok.io");
            

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("CallStatus", "completed"));
            var response = await client.PostAsync($"/osc/callstatus?address={command.Address}&port={command.Port}&argument={(command.Argument?.ToString() ?? string.Empty)}", new FormUrlEncodedContent(keyValues));
        }

        [HttpGet]
        [Route("HandleVoicePromptResponse")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> HandleVoicePromptResponse([FromQuery] string CallSid, [FromQuery] string UnstableSpeechResult, [FromQuery] int SequenceNumber, [FromQuery] decimal Stability, [FromQuery] int promptIndex, [FromQuery] Voice voice)
        {   
            if (promptIndex < 2)
            {
                await GenerateDelusion(UnstableSpeechResult, SequenceNumber, Stability, promptIndex, voice, CallSid);
            }

            return new ContentResult
            {
                Content = string.Empty,
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        [HttpGet]
        [Route("EndGather")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EndGather(int promptIndex, Voice voice)
        {
            if (promptIndex < 2)
            {
                var twiml =
                    new VoiceResponse()
                        .Play(HttpContext.Request.WithPath(_botScriptService.GetRelativeUsedPath(voice),
                            $"remark_{promptIndex}.wav"))
                        .Play(HttpContext.Request.WithPath(_botScriptService.GetRelativeUsedPath(voice),
                            $"delusion_{promptIndex}.wav"))
                        .Play(HttpContext.Request.WithPath(_botScriptService.GetRelativeUsedPath(voice),
                            $"prompt_{promptIndex + 1}.wav"))
                        .Gather(HttpContext.Request
                                .WithPath("Delusion/HandleVoicePromptResponse")
                                .SetQueryParams(new {promptIndex = promptIndex + 1, voice})
                                .ToUri(),
                            HttpContext.Request.WithPath("Delusion/EndGather")
                                .SetQueryParams(new {promptIndex = promptIndex + 1, voice})
                                .ToUri());


                return new ContentResult
                {
                    Content = twiml.ToString(),
                    ContentType = "application/xml",
                    StatusCode = 200
                };
            }
            else
            {
                var twiml =
                    new VoiceResponse()
                        .Play(HttpContext.Request.WithPath(_botScriptService.GetRelativeUsedPath(voice),
                            $"goodbye.wav"));


                return new ContentResult
                {
                    Content = twiml.ToString(),
                    ContentType = "application/xml",
                    StatusCode = 200
                };
            }
        }

        private async Task GenerateDelusion(string UnstableSpeechResult, int SequenceNumber, decimal Stability, int promptIndex, Voice voice, string callId)
        {
            if (!string.IsNullOrWhiteSpace(UnstableSpeechResult))
            {
                UnstableSpeechResult = new string(UnstableSpeechResult.Split().First().Where(c => !char.IsPunctuation(c)).ToArray());

                var priorInputCacheKey = GetPriorInputCacheKey(promptIndex, voice, callId);
                var priorInputs = _memoryCache.Get<List<ProcessedSpeechFragment>>(priorInputCacheKey);

                _logger.LogInformation("thinking about delusions for {UnstableSpeechResult} {SequenceNumber} {Stability}", UnstableSpeechResult, SequenceNumber, Stability);

                
                if (priorInputs == null || IsBetterResult(SequenceNumber, Stability, priorInputs))
                {
                    if (priorInputs == null)
                    {
                        priorInputs = new List<ProcessedSpeechFragment>();
                    }

                    _logger.LogInformation($"Passed cache check for {UnstableSpeechResult} {SequenceNumber} {Stability}!", UnstableSpeechResult, SequenceNumber, Stability);

                    var currentInput = new ProcessedSpeechFragment { SequenceNumber = SequenceNumber, Stability = Stability };
                    priorInputs.Add(currentInput);

                    _memoryCache.Set(priorInputCacheKey, priorInputs);

                    DelusionType[] delusions = ReadDelusionsScript(voice);

                    var delusionPath = Path.Combine(_botScriptService.GetFullUsedPath(voice), $"delusion_{promptIndex}.wav");
                    
                    var audioData = await AudioDataStream(voice, UnstableSpeechResult, delusions[promptIndex]);

                    priorInputs = _memoryCache.Get<List<ProcessedSpeechFragment>>(priorInputCacheKey);

                    //in the time it took to generate this delusion we might have had a better result, and if so we can throw away
                    //this stream and leave the file with the delusion generated based on the better result
                    if (!priorInputs.Any(p => p.SequenceNumber > SequenceNumber && p.Stability >= Stability))
                    {
                        await audioData.SaveToWaveFileAsync(delusionPath);
                        _logger.LogInformation("generated a delusion for {UnstableSpeechResult} {SequenceNumber} {Stability}!", UnstableSpeechResult, SequenceNumber, Stability);
                    }
                    else
                    {
                        _logger.LogInformation("skipped generating a delusion for {UnstableSpeechResult} {SequenceNumber} {Stability}!", UnstableSpeechResult, SequenceNumber, Stability);
                    }
                }
            }
        }

        private static bool IsBetterResult(int SequenceNumber, decimal Stability, List<ProcessedSpeechFragment> priorInputs)
        {
            return (Stability >= priorInputs.Max(p => p.Stability) && SequenceNumber > priorInputs.Max(p => p.SequenceNumber));
        }

        private static string GetPriorInputCacheKey(int promptIndex, Voice voice, string callId)
        {
            return $"prior-input-{voice}-{promptIndex}-{callId}";
        }

        private DelusionType[] ReadDelusionsScript(Voice voice)
        {
            var delusionScriptPath = Path.Combine(_botScriptService.GetFullUsedPath(voice), "delusions.json");
            JObject delusionTypes = JObject.Parse(System.IO.File.ReadAllText(delusionScriptPath));
            var delusions = ((JArray)delusionTypes["Delusions"]).Select(a => (DelusionType)a.Value<int>()).ToArray();
            return delusions;
        }

        private async Task<AudioDataStream> AudioDataStream(Voice voice, string word, DelusionType? delusionType)
        {
            var endConcept = _delusionDictionary.RandomConcept(delusionType.Value);
            var associations = _associationFormatter.Format(await _conceptGraphDb.GetAssociations(word, endConcept));
            var delusionDescription = _delusionDictionary.DescribeDelusion(delusionType.Value);
            var output = $"{associations} {delusionDescription}";

            var audioStream = await _speechService.SpeakText(output, voice);
            return audioStream;
        }
    }
}