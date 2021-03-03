using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Hands
{
    public class ThreeOfAKind : IRules
    {
        private const int Rate = 170;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();
            
            var allCards = tableCards.Concat(playerHand).ToList();

            if (allCards.Any(c => c.Rank is CardRankType.Joker)) 
                CheckJokers(allCards);

            foreach (var card in allCards)
            {
                var possibleThreeOfAKind = allCards
                    .Where(c => c.Rank == card.Rank)
                    .ToList();
                
                if (possibleThreeOfAKind.Count is 3)
                {
                    result.IsWinningHand = true;
                    result.EvaluatedHand.HandType = HandType.ThreeOfAKind;
                    result.EvaluatedHand.Value = (int) card.Rank * 3 * Rate;
                    result.EvaluatedHand.Cards = possibleThreeOfAKind.ToList();

                    // Reset jokers
                    foreach (var cardToAdd in result.EvaluatedHand.Cards.Where(cardToAdd => cardToAdd.WasJoker))
                        cardToAdd.Rank = CardRankType.Joker;
                    
                    foreach (var cardToRemove in possibleThreeOfAKind) 
                        allCards.Remove(cardToRemove);
                    
                    AddSideCards(result.EvaluatedHand.Cards, allCards);
                    
                    break;
                }
            }

            return result;
        }

        private void CheckJokers(List<Card> cards)
        {
            // If there is 1 joker -> check if there is a pair
            // If there are 2 jokers -> find a card with the highest value
            
            var numberOfJokers = cards.Count(c => c.Rank is CardRankType.Joker);
        
            if (numberOfJokers is 1)
                CheckWithOneJoker(cards);
            else
                CheckWithTwoJokers(cards);
        }
        
        private void CheckWithOneJoker(List<Card> cards)
        {
            foreach (var card in cards)
            {
                if (cards.FindAll(c => c.Rank == card.Rank).Count is 2)
                {
                    var joker = cards.First(c => c.Rank is CardRankType.Joker);
                    
                    joker.Rank = card.Rank;
                    joker.WasJoker = true;
                    return;
                }
            }
        }
        
        private void CheckWithTwoJokers(List<Card> cards)
        {
            foreach (var card in cards.Where(c => c.Rank is CardRankType.Joker))
            {
                card.Rank = (CardRankType)GetMaxValue(cards);
                card.WasJoker = true;
            }
        }

        private int GetMaxValue(List<Card> cards)
        {
            var maxValue = 0;
        
            foreach (var card in cards)
            {
                if (maxValue < (int)card.Rank)
                {
                    if (card.Rank is not CardRankType.Joker)
                        maxValue = (int)card.Rank;
                }
            }
        
            return maxValue;
        }
        
        private void AddSideCards(List<Card> finalCardsList, List<Card> allCards)
        {
            allCards = allCards
                .OrderByDescending(c => c.Rank)
                .ToList();
            
            for (var index = 0; index < 2; index++)
                finalCardsList.Add(allCards[index]);
        }
        
    }
}
