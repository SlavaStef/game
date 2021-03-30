using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Helpers.BotLogic.Interfaces;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.BusinessLogic.Services;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Helpers.BotLogic
{
    public class HardBotLogic : IBotLogic
    {
        private readonly ICardEvaluationService _cardEvaluationService;
        private readonly ILogger<BotService> _logger;

        private static Random _random;
        private static bool _isStep3GoodCombination;
        private static PlayerActionType _thirdRoundChoice;

        public HardBotLogic(ICardEvaluationService cardEvaluationService, Random random, ILogger<BotService> logger)
        {
            _cardEvaluationService = cardEvaluationService;
            _random = random;
            _logger = logger;
        }

        public PlayerAction Act(Player bot, Table table)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }

        private PlayerAction ActOnFirstRound(Player bot, Table table)
        {
            try
            {
                var betAmount = table.CurrentMaxBet - bot.CurrentBet;

                if (bot.PocketCards[0].Suit == bot.PocketCards[1].Suit ||
                    Math.Abs((int) bot.PocketCards[0].Rank - (int) bot.PocketCards[1].Rank) is 1)
                {
                    switch (betAmount)
                    {
                        case > 0 when bot.StackMoney * 0.9 > betAmount:
                            return TryActCall(bot, table);
                        case 0 when _random.Next(1, 100) < 20 && bot.StackMoney > table.BigBlind:
                            return new PlayerAction
                            {
                                ActionType = PlayerActionType.Raise, Amount = table.BigBlind,
                                PlayerIndexNumber = bot.IndexNumber
                            };
                        case 0:
                            return new PlayerAction
                                {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    }
                }

                if (bot.HandValue < 12 && betAmount > table.BigBlind * 2)
                    return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};

                var randomPair = _random.Next(2, 14) * 10;

                if (bot.HandValue > randomPair)
                {
                    if (_random.Next(1, 100) < 50)
                    {
                        if (betAmount > 0)
                            return TryActCall(bot, table);

                        var myBet = _random.Next(1, 100) < 50
                            ? table.BigBlind
                            : table.BigBlind * 2;

                        if (myBet <= bot.StackMoney)
                            return new PlayerAction
                            {
                                ActionType = PlayerActionType.Raise, Amount = myBet,
                                PlayerIndexNumber = bot.IndexNumber
                            };
                    }
                    else if (betAmount is 0)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                }

                return betAmount is 0
                    ? new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber}
                    : TryActCall(bot, table);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }

        private PlayerAction ActOnSecondRound(Player bot, Table table)
        {
            try
            {
                var betAmount = table.CurrentMaxBet - bot.CurrentBet;
                var reached75 = false;

                var groups = bot.PocketCards
                    .Select(c => (int) c.Suit)
                    .GroupBy(v => v)
                    .ToList();

                foreach (var group in groups)
                    if (group.Count() is 4)
                        reached75 = true;

                if (bot.Hand is HandType.ThreeOfAKind || bot.Hand is HandType.TwoPairs)
                    reached75 = true;

                if (reached75)
                {
                    if (betAmount is 0)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    if (betAmount < bot.StackMoney * 0.7)
                        return TryActCall(bot, table);
                }

                if (betAmount is 0 && bot.HandValue < 1020)
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};

                if (bot.HandValue < 15)
                {
                    if (betAmount > table.BigBlind)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};

                    if (betAmount > 0)
                        return TryActCall(bot, table);

                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                }

                if (bot.Hand is HandType.OnePair || bot.Hand is HandType.TwoPairs)
                {
                    if (betAmount > 0 && betAmount <= bot.StackMoney * 0.2f)
                    {
                        return TryActCall(bot, table);
                    }

                    if (betAmount is 0)
                    {
                        if (_random.Next(1, 100) < 50)
                            return new PlayerAction
                                {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};

                        float myBet = table.BigBlind * (1 + _random.Next(0, 25) / 100);

                        if (myBet <= bot.StackMoney)
                            return new PlayerAction
                            {
                                ActionType = PlayerActionType.Raise, Amount = (int) myBet,
                                PlayerIndexNumber = bot.IndexNumber
                            };
                    }

                    return TryActCall(bot, table);
                }

                if (bot.HandValue >= 1020) // Triple of 2
                {
                    if (betAmount is 0)
                    {
                        var myBet = _random.Next(table.BigBlind, table.Pot.TotalAmount);

                        if (myBet is 0)
                            myBet = (int) (bot.StackMoney * 0.2f);

                        if (myBet is 0)
                            return new PlayerAction
                                {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};

                        if (myBet < table.BigBlind)
                            myBet = table.BigBlind;

                        if (myBet <= bot.StackMoney)
                            return new PlayerAction
                            {
                                ActionType = PlayerActionType.Raise, Amount = myBet,
                                PlayerIndexNumber = bot.IndexNumber
                            };
                    }
                    else
                        return TryActCall(bot, table);
                }

                if (betAmount > 0 && betAmount < bot.StackMoney * 0.1f)
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};

                return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }

        private PlayerAction ActOnThirdRound(Player bot, Table table)
        {
            try
            {
                var betAmount = table.CurrentMaxBet - bot.CurrentBet;

                _thirdRoundChoice = PlayerActionType.None;
                _isStep3GoodCombination = false;
                var reached75 = false;

                var groups = bot.PocketCards
                    .Select(c => (int) c.Suit)
                    .GroupBy(v => v);

                foreach (var group in groups)
                    if (group.Count() is 4)
                        reached75 = true;

                if (bot.Hand is HandType.ThreeOfAKind)
                    reached75 = true;
                if (bot.Hand is HandType.TwoPairs)
                    reached75 = true;

                if (reached75)
                {
                    if (_random.Next(1, 100) < 30)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
                    if (betAmount is 0)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    if (betAmount < bot.StackMoney * 0.6)
                        return TryActCall(bot, table);
                }

                if (bot.Hand is HandType.OnePair)
                {
                    if (betAmount is 0)
                    {
                        _thirdRoundChoice = PlayerActionType.Check;
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    }

                    if (betAmount > 0 && betAmount <= table.BigBlind * 3)
                        return TryActCall(bot, table);
                }

                if (bot.HandValue >= 1020) // Triple of 2
                {
                    _isStep3GoodCombination = true;
                    if (betAmount is 0)
                    {
                        if (_random.Next(1, 100) < 50)
                        {
                            _thirdRoundChoice = PlayerActionType.Check;
                            return new PlayerAction
                                {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                        }

                        var myBet = table.Pot.TotalAmount > bot.StackMoney
                            ? bot.StackMoney
                            : _random.Next(table.Pot.TotalAmount, bot.StackMoney + 1);

                        if (myBet < table.BigBlind)
                            myBet = table.BigBlind;
                        if (myBet >= table.BigBlind && myBet <= bot.StackMoney)
                        {
                            _thirdRoundChoice = PlayerActionType.Raise;
                            return new PlayerAction
                            {
                                ActionType = PlayerActionType.Raise, Amount = myBet,
                                PlayerIndexNumber = bot.IndexNumber
                            };
                        }
                    }
                    else
                        return TryActCall(bot, table);
                }

                if (betAmount > 0 && betAmount < bot.StackMoney * 0.1f)
                    return new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};

                return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }

        private PlayerAction ActOnFourthRound(Player bot, Table table)
        {
            try
            {
                var betAmount = table.CurrentMaxBet - bot.CurrentBet;

                if (betAmount is 0 && bot.StackMoney > 0 && _isStep3GoodCombination)
                {
                    if (_thirdRoundChoice is PlayerActionType.Check)
                    {
                        var myBet = _random.Next(1, 100) < 50
                            ? _random.Next(table.BigBlind, bot.StackMoney / 2)
                            : bot.StackMoney;
                        if (myBet >= table.BigBlind && myBet <= bot.StackMoney)
                            return new PlayerAction
                            {
                                ActionType = PlayerActionType.Raise, Amount = myBet,
                                PlayerIndexNumber = bot.IndexNumber
                            };
                    }
                    else if (_thirdRoundChoice is PlayerActionType.Raise)
                    {
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    }
                }

                if (bot.HandValue < 15)
                {
                    if (betAmount is 0)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
                }

                if (bot.Hand is HandType.OnePair)
                {
                    if (betAmount is 0)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};
                    return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
                }

                if (bot.Hand is HandType.TwoPairs)
                {
                    if (betAmount > 0)
                        return TryActCall(bot, table);

                    if (_random.Next(1, 100) < 50)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};

                    var myBet = _random.Next(table.Pot.TotalAmount / 3, table.Pot.TotalAmount);

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

                if (bot.HandValue > 1020)
                {
                    if (betAmount > 0)
                        return TryActCall(bot, table);

                    if (_random.Next(1, 100) < 50)
                        return new PlayerAction
                            {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};

                    var myBet = 0;
                    
                    if (table.Pot.TotalAmount < bot.StackMoney + 1)
                        _random.Next(table.Pot.TotalAmount, bot.StackMoney + 1);
                    
                    myBet = myBet > bot.StackMoney
                        ? bot.StackMoney
                        : myBet;

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

                if (betAmount > 0 && betAmount < bot.StackMoney * 0.1f)
                    return new PlayerAction {ActionType = PlayerActionType.Check, PlayerIndexNumber = bot.IndexNumber};

                return new PlayerAction {ActionType = PlayerActionType.Fold, PlayerIndexNumber = bot.IndexNumber};
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }

        private PlayerAction TryActCall(Player bot, Table table)
        {
            return bot.StackMoney < table.CurrentMaxBet 
                ? new PlayerAction {ActionType = PlayerActionType.AllIn, PlayerIndexNumber = bot.IndexNumber} 
                : new PlayerAction {ActionType = PlayerActionType.Call, PlayerIndexNumber = bot.IndexNumber};
        }
    }
}