
namespace DelusionalApi.Model.Bots
{
    public class FelicityBot : IBot
    {
        public Voice Voice => Voice.Felicity;

        public DelusionType[] Delusions => new [] { DelusionType.Impregnate, DelusionType.Intruders, DelusionType.Wires, DelusionType.Witchcraft };

        public string Intro(RandomString randomString)
        {
            return "Your conscious thoughts are no longer your own. There is only one way I can attempt to reach your authentic self now. I say a word, then you say the first word that comes into your head. Got it?";
        }

        public string Prompt(int promptIndex, RandomString randomString)
        {
            var think = randomString.Next("think", "imagine", "want to say", "respond with", "come up with");
            var prompt = $"What do you {think} when I say {randomString.Next()}";

            if (promptIndex > 0)
            {
                var fun = randomString.Next("fun", "interesting", "amusing", "entertaining", "amazing", "engaging", "illuminating", "addictive", "mesmerizing", "edifying");
                var listen = randomString.Next("listen", "pay attention", "focus", "pay attention", "follow me", "stay with me", "focus", "hear me", "concentrate", "hear me");

                prompt = $"See? The game is {fun}! Now {listen}. {prompt}";
            }
            else
            {
                prompt = $"{randomString.Next("Ok, let's do this!", "I hope you get it!")} {prompt}";
            }

            return prompt;
        }
        
        public string Goodbye(RandomString randomString)
        {
            var disturbing = randomString.Next("disturbing", "skewed", "evil", "messed up", "sick", "screwed up", "fucked", "disgusting", "stupid", "crazy", "insane", "wonky");
            return $"Okay that's enough. You know I'm a champion kickboxer. I'm going to hang up now and soon we'll see whether you'll say these {disturbing} words to my face.";
        }

        public string InterstitialRemark(int promptIndex, RandomString randomString)
        {
            return randomString.Next("It's funny. You look 20 years older than you are, but that response is so childish.",
                "If you keep up these kind of responses, I'll do to you what I've done to all the other arseholes.",
                "I've been sleep deprived for 72 hours because of you. Are you trying to make up for that now by boring me to death?");
        }
    }
}