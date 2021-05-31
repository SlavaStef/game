using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Player;
using PokerHand.Common.Helpers.Present;
using PokerHand.Common.Entities.Chat;

namespace PokerHand.Common.Entities
{
    public class Player : IdentityUser<Guid>
    {
        // Properties to store in db
        public Gender Gender { get; set; }
        public CountryCode Country { get; set; }
        public DateTime RegistrationDate { get; set; }
        public HandsSpriteType HandsSprite { get; set; }
        public int TotalMoney { get; set; }
        public int CoinsAmount { get; set; }
        public int MoneyBoxAmount { get; set; }
        public int Experience { get; set; }
        public int GamesPlayed { get; set; }
        public HandType BestHandType { get; set; }
        public int GamesWon { get; set; }
        public int BiggestWin { get; set; }
        public int SitAndGoWins { get; set; }
        public ExternalLogin PlayerLogin { get; set; }
        public string PersonalCode { get; set; }
        public string Friends { get; set; }

        public List<Message> Messages { get; set; }
        public virtual List<Conversation> ConversationsFirst { get; set; }
        public virtual List<Conversation> ConversationsSecond { get; set; }

        // Game properties
        [NotMapped] public string ConnectionId { get; set; }
        [NotMapped] public PresentName PresentName { get; set; }
        [NotMapped] public bool IsAutoTop { get; set; }
        [NotMapped] public int CurrentBuyIn { get; set; }
        [NotMapped] public List<Card> PocketCards { get; set; }
        [NotMapped] public PlayerAction CurrentAction { get; set; }
        [NotMapped] public int CurrentBet { get; set; }
        [NotMapped] public int StackMoney { get; set; }
        [NotMapped] public int IndexNumber { get; set; }
        [NotMapped] public HandType Hand { get; set; }
        [NotMapped] public int HandValue { get; set; }
        [NotMapped] public List<Card> HandCombinationCards { get; set; }
        [NotMapped] public bool IsReady { get; set; }
        [NotMapped] public int NumberOfGamesOnCurrentTable { get; set; }
    }
}