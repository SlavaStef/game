namespace PokerHand.Common.Helpers.Bot
{
    public class Bot : Entities.Player
    {
        public BotComplexity Complexity { get; set; }
        public string ImageUri { get; set; }
    }
}