using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Dto
{
    public class SidePotDto
    {
        public SidePotType Type { get; set; }
        public List<Player> Winners { get; set; }
        public int WinningAmountPerPlayer { get; set; }
    }
}