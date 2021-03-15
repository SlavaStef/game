namespace PokerHand.Common.Helpers.GameProcess
{
    public class PlayerAction
    {
        public int PlayerIndexNumber { get; set; }
        public PlayerActionType ActionType { get; set; }
        public int? Amount { get; set; }
    }
}