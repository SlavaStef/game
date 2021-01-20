using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class Player : IdentityUser<Guid>
    {
        // Properties to store in db
        public string Country { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int Experience { get; set; }
        public int TotalMoney { get; set; }
        public int CoinsAmount { get; set; }
        public int GamesPlayed { get; set; }
        public HandType BestHandType { get; set; }
        public int GamesWon { get; set; }
        public int BiggestWin { get; set; }
        public int SitAndGoWins { get; set; }
        
        // Game properties
        [NotMapped] 
        public string ConnectionId { get; set; }
        
        [NotMapped]
        public bool IsAutoTop { get; set; }

        [NotMapped]
        public int CurrentBuyIn { get; set; }
        
        [NotMapped]
        public List<Card> PocketCards { get; set; }
        
        [NotMapped]
        public PlayerAction CurrentAction { get; set; }
        
        [NotMapped]
        public int CurrentBet { get; set; }
        
        [NotMapped]
        public int StackMoney { get; set; } // The total chips and currency that a player has in play at a given moment
        
        [NotMapped]
        public ButtonTypeNumber Button { get; set; }
        
        [NotMapped]
        public int IndexNumber { get; set; } // Place at the table
        
        [NotMapped]
        public HandType Hand;
        
        [NotMapped] 
        public int HandValue { get; set; }
        
        [NotMapped] 
        public List<Card> HandCombinationCards { get; set; }
        
        [NotMapped]
        public bool IsReady { get; set; }
    }
}