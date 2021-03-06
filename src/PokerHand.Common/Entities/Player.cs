﻿using System;
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
        public PlayerAction CurrentAction { get; set; }
        public int CurrentBet { get; set; }
        public int TotalMoney { get; set; }
        public int StackMoney { get; set; } // The total chips and currency that a player has in play at a given moment
        public ButtonTypeNumber Button { get; set; }
        public int IndexNumber { get; set; } // Place at the table
        public HandType Hand;
    }
}