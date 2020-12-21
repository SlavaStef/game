using System;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Dto
{
    /*
     * PlayerProfileDto is sent to client in order
     * to display player's statistics and profile info.
     * Field "Id" is currently used to authenticate client
     */
    
    public class PlayerProfileDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
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
    }
}