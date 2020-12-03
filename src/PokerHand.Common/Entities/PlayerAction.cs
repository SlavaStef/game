using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class PlayerAction
    {
        public int PlayerIndexNumber { get; set; }
        public PlayerActionType ActionType { get; set; }
        public int? Amount { get; set; }
    }
}