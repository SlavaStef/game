using System.Collections.Generic;

namespace PokerHand.Common.Helpers
{
    public class EvaluatedHand
    {
        public int Value { get; set; }
        public HandType HandType { get; set; }
        public List<Entities.Card> Cards { get; set; }
        
        public EvaluatedHand()
        {
            Value = 0;
            HandType = HandType.None;
            Cards = null;
        }
    }
}