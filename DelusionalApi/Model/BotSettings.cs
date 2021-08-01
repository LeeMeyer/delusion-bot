using System.Collections.Generic;

namespace DelusionalApi.Model
{
    public class BotSettings
    {
        public string Phone { get; set; }
        public List<Osc> OscMessages { get; set; }
        public int CallDelayInSeconds { get; set; }
    }
}