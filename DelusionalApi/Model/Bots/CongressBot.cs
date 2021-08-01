
namespace DelusionalApi.Model.Bots
{
    public class CongressBot : IBot
    {
        public Voice Voice => Voice.Phil;

        public DelusionType[] Delusions => new [] { DelusionType.Impregnate, DelusionType.Intruders, DelusionType.Wires, DelusionType.Witchcraft };

        public string Intro(RandomString randomString)
        {
            return "I am calling you on behalf of the world congress. We cannot run the risk that incorrect views of the world pervade society, so you have been randomly selected for a short test. No pressure. Think of this as a game. I say a word, then you say the next word that comes into your head. Is that clear?";
        }

        public string Prompt(int promptIndex, RandomString randomString)
        {
            var think = randomString.Next("think", "imagine", "want to say", "respond with", "come up with");
            var prompt = $"What do you {think} when I say {randomString.Next()}";

            if (promptIndex > 0)
            {
                var fun = randomString.Next("fun", "interesting", "amusing", "entertaining", "amazing", "engaging", "illuminating", "addictive", "mesmerizing", "edifying");
                var listen = randomString.Next("listen", "pay attention", "focus", "pay attention", "follow me", "stay with me", "focus", "hear me", "concentrate", "hear me");

                prompt = $"I trust you are finding that the game is {fun}! Now {listen}. {prompt}";
            }
            else
            {
                prompt = $"{randomString.Next("Ok, let's begin", "I hope you get it!")} {prompt}";
            }

            return prompt;
        }
        
        public string Goodbye(RandomString randomString)
        {
            var disturbing = randomString.Next("disturbing", "skewed", "evil", "messed up", "sick", "screwed up", "fucked", "disgusting", "stupid", "crazy", "insane", "wonky");
            return $"I see. As you know, I am only a messenger. My personal view that your responses reveal {disturbing} thought patterns, is besides the point. We thank you for your time, and will come back to you with a determination in due course.";
        }

        public string InterstitialRemark(int promptIndex, RandomString randomString)
        {
            if (promptIndex == 0)
            {
                return "There are no right or wrong answers here. But your response triggers strange associations that may take some time to process.";
            }

            return randomString.Next("Why do people spread rumors of the sonic weapon medusa, when the ultimate sonic weapon is the horrible sound of your voice?",
                "As much as we cannot have people spreading far fetched ideas, your response is so unoriginal I find myself thinking delusions are underrated",
                "I had to wade through thousands of dusty legal texts to get where I am today. Yet your response is the most uninspired thing I have ever heard.");
        }
    }
}