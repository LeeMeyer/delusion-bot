using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelusionalApi.Model
{
    public class ProcessedSpeechFragment
    {
        public int SequenceNumber { get; set; }
        public decimal Stability { get; set; }
    }
}
