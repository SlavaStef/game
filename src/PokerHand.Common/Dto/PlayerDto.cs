using System;
using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Dto
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public List<Card> PocketCards { get; set; }
        public int CurrentBet { get; set; }
        public int TotalMoney { get; set; }
        public int StackMoney { get; set; }
        public ButtonTypeNumber Button { get; set; }
        public int IndexNumber { get; set; } // place at the table
    }
}