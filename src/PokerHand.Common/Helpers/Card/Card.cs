namespace PokerHand.Common.Helpers.Card
{
    public class Card
    {
        public CardRankType Rank { get; set; }
        public CardSuitType Suit { get; set; }
        public Card SubstitutedCard { get; set; }
    }
}