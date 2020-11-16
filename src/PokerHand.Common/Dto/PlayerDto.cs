using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Dto
{
    public class PlayerDto
    {
        public List<Card> HandCards { get; set; }
        public int CurrentBet { get; set; }
        public int TotalMoney { get; set; }
        public int CurrentMoney { get; set; }
        public ButtonTypeNumber Button { get; set; }
    }
}