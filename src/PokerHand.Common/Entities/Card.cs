using PokerHand.Common.Helpers.Card;

namespace PokerHand.Common.Entities
{
    public class Card
    {
        public CardRankType Rank { get; set; }
        public CardSuitType Suit { get; set; }
        public bool WasJoker { get; set; } 
    }
}