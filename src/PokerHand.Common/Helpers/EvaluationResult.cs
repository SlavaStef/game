using System.Collections.Generic;
using PokerHand.Common.Entities;

namespace PokerHand.Common.Helpers
{
    public class EvaluationResult
    {
        public bool IsWinningHand { get; set; }
        public EvaluatedHand EvaluatedHand { get; set; }

        public EvaluationResult()
        {
            IsWinningHand = false;
            EvaluatedHand = new EvaluatedHand();
        }
    }
}