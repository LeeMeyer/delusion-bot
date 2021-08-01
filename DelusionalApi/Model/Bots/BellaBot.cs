
namespace DelusionalApi.Model.Bots
{
    public class BellaBot : IBot
    {
        public Voice Voice => Voice.Bella;

        public DelusionType[] Delusions => new [] { DelusionType.Impregnate, DelusionType.Intruders, DelusionType.Wires, DelusionType.Witchcraft };

        public string Intro(RandomString randomString)
        {
            return "It's me. The penis is symbolised in psychiatry by the snake. Psychiatrists play a game with their patients. I say a word, then you say the first word that comes into your head. Got it?";
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
            return $"Look, I'm only a child, and the way you think is {disturbing} and I'm hanging up.";
        }

        public string InterstitialRemark(int promptIndex, RandomString randomString)
        {
            if (promptIndex == 0)
            {
                return "There are no right or wrong answers here. But your response triggers strange associations that may take some time to process.";
            }

            return randomString.Next("I remember a time before I could even speak. You've had vocal chords for longer than me, yet you use them to respond to me with drivel.",
                "Sometimes I hear my past souls talk to me. If only they would drown out your inane response.",
                "You and I are just actors in a movie. Often we read a script. Right now we are improvising. Nevertheless, your responses belong on the blooper reel.",
                "I suspect you of cheating using a dictionary of boring and irrelevant terms. At least give me some messed up words to avoid putting me to sleep");
        }
    }
}