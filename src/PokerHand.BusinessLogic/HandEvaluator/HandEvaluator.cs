using System.Collections.Generic;
using PokerHand.BusinessLogic.HandEvaluator.Hands;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.HandEvaluator
{
    public class HandEvaluator
    {
        private IRules _currentRules;
        private readonly List<IRules> _listRules;
        private bool _result;

        public HandEvaluator()
        {
            _listRules = new List<IRules>
            {
                new RoyalFlush(),
                new StraightFlush(),
                new FourOfAKind(),
                new FullHouse(),
                new Flush(),
                new Straight(),
                new ThreeOfAKind(),
                new TwoPairs(),
                new OnePair(),
                new HighCard()
            };
        }

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame ,out int value, out HandType handType, out List<Card> winnerCards)
        {
            value = 0;
            handType = HandType.None;
            winnerCards = new List<Card>(7);

            foreach (var rule in _listRules)
            {
                _currentRules = rule;
                _result = _currentRules.Check(playerHand, tableCards, isJokerGame, out value, out handType, out winnerCards);
                if (_result) break;
            }

            return _result;
        }
    }
}