using System.Collections.Generic;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class SidePot
    {
        public SidePotType Type { get; set; }
        public int TotalAmount { get; set; }
        public List<Player> Players { get; set; }
        public int WinningAmountPerPlayer { get; set; }
        public List<Player> Winners { get; set; }

        public SidePot()
        {
            Type = SidePotType.Side;
            TotalAmount = 0;
            Players = new List<Player>();
            WinningAmountPerPlayer = 0;
            Winners = new List<Player>();
        }
    }
}