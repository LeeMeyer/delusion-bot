using System.Collections.Generic;

namespace DelusionalApi.Model
{
    public interface IBot
    {
        public Voice Voice { get; }
        public DelusionType[] Delusions { get; }
        public string Intro(RandomString randomString);
        public string Prompt(int promptIndex, RandomString randomString);
        public string Goodbye(RandomString randomString);
        public string InterstitialRemark(int promptIndex, RandomString randomString);
    }
}