using System;
using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.GameProcess;

namespace PokerHand.Common.Dto
{
    /*
     * PlayerDto sends to client during game process.
     * It contains information, related only to game process
     */
    
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public List<Card> PocketCards { get; set; }
        public PlayerAction CurrentAction { get; set; }
        public int CurrentBet { get; set; }
        public int StackMoney { get; set; }
        public ButtonTypeNumber Button { get; set; }
        public int IndexNumber { get; set; }
        public HandType Hand;
        public bool IsReady { get; set; }
    }
}