using System.Collections.Generic;

namespace PokerHand.Common.Helpers.CardEvaluation
{
    public class Hand
    {
        public int Value { get; set; }
        public HandType HandType { get; set; }
        public List<Card.Card> Cards { get; set; }
        
        public Hand()
        {
            Value = 0;
            HandType = HandType.None;
            Cards = null;
        }
    }
}