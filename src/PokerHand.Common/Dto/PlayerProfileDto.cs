using System;
using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.Player;

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
        public Gender Gender { get; set; }
        public CountryCode Country { get; set; }
        public DateTime RegistrationDate { get; set; }
        public HandsSpriteType HandsSprite { get; set; }
        public int TotalMoney { get; set; }
        public int MoneyBoxAmount { get; set; }
        public int Experience { get; set; }
        public int GamesPlayed { get; set; }
        public HandType BestHandType { get; set; }
        public int GamesWon { get; set; }
        public int BiggestWin { get; set; }
        public int SitAndGoWins { get; set; }
        public List<ExternalLoginDto> PlayerLogins { get; set; }
    }
}