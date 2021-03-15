namespace PokerHand.Common.Helpers.CardEvaluation
{
    public class EvaluationResult
    {
        public bool IsWinningHand { get; set; }
        public Hand Hand { get; set; }

        public EvaluationResult()
        {
            IsWinningHand = false;
            Hand = new Hand();
        }
    }
}