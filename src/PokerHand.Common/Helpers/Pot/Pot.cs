using System;
using System.Collections.Generic;

namespace PokerHand.Common.Helpers.Pot
{
    public class Pot
    {
        public int TotalAmount { get; set; }
        public Dictionary<Guid, int> Bets { get; set; }

        public Pot()
        {
            Bets = new Dictionary<Guid, int>();
            TotalAmount = 0;
        }
    }
}