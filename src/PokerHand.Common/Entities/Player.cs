using System;
using System.Collections.Generic;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class Player
    {
        public Player()
        {
            Id = Guid.NewGuid();
            Button = ButtonTypeNumber.None;
        }
        
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public List<Card> PocketCards { get; set; }
        public int CurrentBet { get; set; } // Any money wagered during the play of a hand
        public int TotalMoney { get; set; }
        public int StackMoney { get; set; } // The total chips and currency that a player has in play at a given moment
        public ButtonTypeNumber Button { get; set; }
        public int IndexNumber { get; set; }
    }
}