using Catalyst;

namespace DelusionalApi.Model
{
    public class TokenPrompt
    {
        public IToken Token { get; set; }
        public IToken NeighbouringToken { get; set; }
    }
}
