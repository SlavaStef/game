using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.Common.Dto
{
    public class SidePotDto
    {
        public SidePotType Type { get; set; }
        public List<PlayerDto> Players { get; set; }
        public List<PlayerDto> Winners { get; set; }
        public int WinningAmountPerPlayer { get; set; }
    }
}