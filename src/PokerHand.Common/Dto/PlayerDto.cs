using System;
using System.Collections.Generic;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.Player;
using PokerHand.Common.Helpers.Present;

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
        public HandsSpriteType HandsSprite { get; set; }
        public List<Card> PocketCards { get; set; }
        public int CurrentBet { get; set; }
        public int StackMoney { get; set; }
        public int IndexNumber { get; set; }
        public HandType Hand { get; set; }
        public Present Present { get; set; }
    }
}