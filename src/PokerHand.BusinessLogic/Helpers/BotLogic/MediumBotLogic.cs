using System;
using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.BotLogic.Interfaces;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Helpers.BotLogic
{
    public class MediumBotLogic : IBotLogic
    {
        private readonly ICardEvaluationService _cardEvaluationService;
        private static readonly Random Random = new();
        private static bool _isStep3GoodCombination;
        private static PlayerActionType _thirdRoundChoice = PlayerActionType.None;

        public MediumBotLogic(ICardEvaluationService cardEvaluationService)
        {
            _cardEvaluationService = cardEvaluationService;
        }

        public PlayerAction Act(Player bot, Table table)
        {
            bot = _cardEvaluationService.EvaluatePlayerHand(table.CommunityCards, bot);

            return table.CurrentStage switch
            {
                RoundStageType.WageringPreFlopRound => ActOnFirstRound(bot, table),
                RoundStageType.WageringSecondRound => ActOnSecondRound(bot, table),
                RoundStageType.WageringThirdRound => ActOnThirdRound(bot, table),
                _ => ActOnFourthRound(bot, table)
            };
        }

        private static PlayerAction ActOnFirstRound(Player bot, Table table)
        {
            var betAmount = table.CurrentMaxBet - bot.CurrentBet;
            
            if (bot.PocketCards[0].Suit == bot.PocketCards[1].Suit ||
                Math.Abs((int) bot.PocketCards[0].Rank - (int) bot.PocketCards[1].Rank) is 1)
            {
                if (betAmount > 0 && bot.StackMoney * 0.9 > betAmount)
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
                
                if (betAmount is 0)
                {
                    if (Random.Next(1, 100) < 20 && bot.StackMoney >= table.BigBlind)
                        return new PlayerAction
                        {
                            ActionType = PlayerActionType.Raise, Amount = table.BigBlind,
                            PlayerIndexNumber = bot.IndexNumber
                        };
                    else
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                }
            }

            if (bot.HandValue < 12 && betAmount > table.BigBlind * 2)
                return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};

            var randomPair = Random.Next(2, 14) * 10;
            
            if (bot.HandValue > randomPair)
            {
                if (Random.Next(1, 100) < 50)
                {
                    if (betAmount > 0)
                        return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
                    
                    var myBet = Random.Next(1, 100) < 50
                        ? table.BigBlind
                        : table.BigBlind * 2;

                    if (myBet <= bot.StackMoney && myBet >= table.BigBlind)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Raise, Amount = myBet, PlayerIndexNumber = bot.IndexNumber};
                    else
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                }
                else if (betAmount is 0)
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
            }

            return betAmount is 0
                ? new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber}
                : new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
        }

        private static PlayerAction ActOnSecondRound(Player bot, Table table)
        {
            var betAmount = table.CurrentMaxBet - bot.CurrentBet;
            
            if (betAmount is 0 && bot.HandValue < 1020)
                return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};

            if (bot.HandValue < 15)
            {
                if (betAmount > table.BigBlind)
                    return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
                else if (betAmount > 0)
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
                else
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
            }

            if (bot.Hand is HandType.OnePair || bot.Hand is HandType.TwoPairs)
            {
                if (betAmount > 0 && betAmount <= bot.StackMoney * 0.2f)
                {
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
                }

                if (betAmount is 0)
                {
                    if (Random.Next(1, 100) < 50)
                        return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    else
                    {
                        float myBet = table.BigBlind * (1 + Random.Next(1, 25) / 100);
                        if (myBet <= bot.StackMoney)
                            return new PlayerAction
                            {
                                ActionType = PlayerActionType.Raise, Amount = (int) myBet,
                                PlayerIndexNumber = bot.IndexNumber
                            };
                    }
                }

                return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
            }

            if (bot.HandValue >= 1020)
            {
                if (betAmount is 0)
                {
                    var myBet = Random.Next(table.BigBlind, table.Pot.TotalAmount / 3);
                    if (myBet is 0)
                        myBet = (int) (bot.StackMoney * 0.2f);
                    if (myBet is 0)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    if (myBet < table.BigBlind)
                        myBet = table.BigBlind;
                    if (myBet <= bot.StackMoney && myBet >= table.BigBlind)
                        return new PlayerAction
                        {
                            ActionType = PlayerActionType.Raise, Amount = myBet,
                            PlayerIndexNumber = bot.IndexNumber
                        };
                }
                else
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
            }

            return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
        }

        private static PlayerAction ActOnThirdRound(Player bot, Table table)
        {
            var betAmount = table.CurrentMaxBet - bot.CurrentBet;

            if (bot.Hand is HandType.OnePair)
            {
                if (betAmount is 0)
                {
                    _thirdRoundChoice = PlayerActionType.Check;
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                }

                if (betAmount > 0 && betAmount <= table.BigBlind * 3)
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
            }

            if (bot.HandValue >= 1020)
            {
                _isStep3GoodCombination = true;
                
                if (betAmount is 0)
                {
                    if (Random.Next(1, 100) < 50)
                    {
                        _thirdRoundChoice = PlayerActionType.Check;
                        return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    }
                    else
                    {
                        var myBet = table.Pot.TotalAmount > bot.StackMoney 
                            ? bot.StackMoney 
                            : Random.Next(table.Pot.TotalAmount, bot.StackMoney + 1);
                        
                        if (myBet < table.BigBlind)
                            myBet = table.BigBlind;
                        
                        if (myBet <= bot.StackMoney && myBet >= table.BigBlind)
                        {
                            _thirdRoundChoice = PlayerActionType.Raise;
                            return new PlayerAction
                            {
                                ActionType = PlayerActionType.Raise, Amount = myBet,
                                PlayerIndexNumber = bot.IndexNumber
                            };
                        }
                    }
                }
                else
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
            }

            return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
        }

        private static PlayerAction ActOnFourthRound(Player bot, Table table)
        {
            var betAmount = table.CurrentMaxBet - bot.CurrentBet;
            
            if (betAmount is 0 && bot.StackMoney > 0 && _isStep3GoodCombination)
            {
                if (_thirdRoundChoice is PlayerActionType.Check)
                {
                    var myBet = Random.Next(1, 100) < 50 
                        ? Random.Next(1, bot.StackMoney / 2) 
                        : bot.StackMoney;

                    if (myBet >= table.BigBlind && myBet <= bot.StackMoney)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Raise, Amount = myBet, PlayerIndexNumber = bot.IndexNumber};
                }
                else if (_thirdRoundChoice is PlayerActionType.Raise)
                {
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                }
            }

            if (bot.HandValue < 15)
            {
                if (betAmount is 0)
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
            }

            if (bot.Hand is HandType.OnePair)
            {
                if (betAmount is 0)
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
            }

            if (bot.Hand is HandType.TwoPairs)
            {
                if (betAmount > 0)
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};

                if (Random.Next(1, 100) < 50)
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                else
                {
                    var myBet = Random.Next(table.Pot.TotalAmount / 3, table.Pot.TotalAmount);

                    if (myBet is 0)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    if (myBet < table.BigBlind)
                        myBet = table.BigBlind;
                    if (myBet >= table.BigBlind && myBet <= bot.StackMoney)
                        return new PlayerAction
                        {
                            ActionType = PlayerActionType.Raise, Amount = myBet,
                            PlayerIndexNumber = bot.IndexNumber
                        };
                }
            }

            if (bot.HandValue > 1020)
            {
                if (betAmount > 0)
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};

                if (Random.Next(1, 100) < 50)
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                else
                {
                    var myBet = Random.Next(table.Pot.TotalAmount, bot.StackMoney + 1);
                    
                    myBet = myBet > bot.StackMoney 
                        ? bot.StackMoney 
                        : myBet;
                    
                    if (myBet is 0)
                        return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    if (myBet < table.BigBlind)
                        myBet = table.BigBlind;
                    if (myBet >= table.BigBlind && myBet <= bot.StackMoney)
                        return new PlayerAction
                        {
                            ActionType = PlayerActionType.Raise, Amount = myBet,
                            PlayerIndexNumber = bot.IndexNumber
                        };
                }
            }

            return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
        }
    }
}