using System.Collections.Generic;

namespace DelusionalApi.Model
{
    public class PhonePoem
    {
        public string PhoneNumber { get; set; } 
        public int ExcerptIndex { get; set; }
        public List<int> TokenIndexes { get; set; }
        public List<string> ReplacementWords { get; set; }
    }
}