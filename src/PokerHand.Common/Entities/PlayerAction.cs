using System;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class PlayerAction
    {
        public Guid PlayerId { get; set; }
        public PlayerActionType ActionType { get; set; }
        public int? Amount { get; set; }
    }
}