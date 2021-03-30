using System.Collections.Generic;

namespace PokerHand.Common.Helpers.Table
{
    public class SidePot
    {
        public SidePotType Type { get; set; }
        public int TotalAmount { get; set; }
        public List<Entities.Player> Players { get; set; }
        public int WinningAmountPerPlayer { get; set; }
        public List<Entities.Player> Winners { get; set; }

        public SidePot()
        {
            Type = SidePotType.Side;
            TotalAmount = 0;
            Players = new List<Entities.Player>();
            WinningAmountPerPlayer = 0;
            Winners = new List<Entities.Player>();
        }
    }
}