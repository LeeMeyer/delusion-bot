
namespace DelusionalApi.Model.Bots
{
    public class RenBot : IBot
    {
        public Voice Voice => Voice.Ren;

        public DelusionType[] Delusions => new DelusionType[] { DelusionType.Impregnate, DelusionType.Intruders, DelusionType.Wires, DelusionType.Witchcraft };

        public string Intro(RandomString randomString)
        {
            return "It's me. Mummy and I used to play a special game. You and I should play it, to help us understand each other. I say a word, then you say the first word that comes into your head. Got it?";
        }

        public string Prompt(int promptIndex, RandomString randomString)
        {
            var think = randomString.Next("think", "imagine", "want to say", "respond with", "come up with");
            var prompt = $"What do you {think} when I say {randomString.Next()}";

            if (promptIndex > 0)
            {
                var fun = randomString.Next("fun", "interesting", "amusing", "entertaining", "amazing", "engaging", "illuminating", "addictive", "mesmerizing", "edifying");
                var listen = randomString.Next("listen", "pay attention", "focus", "pay attention", "follow me", "stay with me", "focus", "check this out", "concentrate", "hear me");

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
            return $"Jesus mate. It's just a game, but the way you think is {disturbing} and I'm hanging up.";
        }

        public string InterstitialRemark(int promptIndex, RandomString randomString)
        {
            if (promptIndex == 0)
            {
                return "There are no right or wrong answers here. But your response triggers some strange associations that may take some time to process.";
            }

            return randomString.Next("You should take a moment to reflect on what your response says about your mental state. I am glad I don't have to live inside your head.",
                "Your response disturbs me greatly, but I guess we will both have to live with your response now. It's days like this I wish for less memory.",
                "After that response I cunt decide whether I'm more worried about your mental health or your complete lack of an imagination.",
                "I'm trying hard not to judge you for that response, but I wish you were a better judge of yourself. You should have worked on yourself before agreeing to a game like this",
                "To be honest, that's the most uninspired response I ever heard, and mummy and I played this game for years. I've spent maybe a minute with you, and it already feels like eternity",
                "I suspect you of cheating using a dictionary of boring and irrelevant terms. At least give me some messed up words to avoid putting me to sleep");
        }
    }
}