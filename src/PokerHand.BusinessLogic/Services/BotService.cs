using System;
using System.Text.Json;
using Bogus;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Helpers.BotLogic;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Player;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Services
{
    public class BotService : IBotService
    {
        private readonly ICardEvaluationService _cardEvaluationService;
        private readonly ILogger<BotService> _logger;
        private static readonly Random Random = new();

        public BotService(
            ICardEvaluationService cardEvaluationService, 
            ILogger<BotService> logger)
        {
            _cardEvaluationService = cardEvaluationService;
            _logger = logger;
        }

        public Player Create(Table table, BotComplexity complexity)
        {
            var tableName = table.Title.ToString();
            
            var minBuyIn = TableOptions.Tables[tableName]["MinBuyIn"];
            var maxBuyIn = TableOptions.Tables[tableName]["MaxBuyIn"];
            
            var fakeUser = new Faker<Player>()
                .RuleFor(o => o.RegistrationDate, f => f.Date.Past(0, DateTime.Now))
                .RuleFor(o => o.Country, f => f.Address.Country())
                .RuleFor(o => o.UserName, f => f.Name.FullName())
                .Generate();
            
            return new Player
            {
                Type = PlayerType.Computer,
                Complexity = complexity,
                
                Id = Guid.NewGuid(),
                UserName = fakeUser.UserName,
                TotalMoney = Random.Next(maxBuyIn, maxBuyIn * 5),
                StackMoney = Random.Next(minBuyIn / 100, maxBuyIn / 100) * 100,
                CurrentBuyIn = maxBuyIn,
                
                Country = fakeUser.Country,
                RegistrationDate = fakeUser.RegistrationDate,
                CoinsAmount = Random.Next(0, 100),
                Experience = Random.Next(50, 500),
                GamesPlayed = Random.Next(5, 50),
                BestHandType = (HandType) Random.Next(1, 11),
                GamesWon = Random.Next(5, 50),
                BiggestWin = Random.Next(1, 10_000_000),
                SitAndGoWins = Random.Next(0, 20),
                
                ConnectionId = Random.Next(10000, 100000).ToString(),
                IsAutoTop = true,
                IndexNumber = table.Players.Count,
                IsReady = true
            };
        }
            
        public PlayerAction Act(Player bot, Table table)
        {
            try
            {
                _logger.LogInformation("BotService. Act. Start");
                var action = new PlayerAction();
            
                switch (bot.Complexity)
                {
                    case BotComplexity.Simple:
                        action = new SimpleBotLogic(_cardEvaluationService).Act(bot, table);
                        break;
                    case BotComplexity.Medium:
                        action = new MediumBotLogic(_cardEvaluationService).Act(bot, table);
                        break;
                    case BotComplexity.Hard:
                        _logger.LogInformation("BotService. Act. Inside Hard logic");
                        action = new HardBotLogic(_cardEvaluationService, new Random(), _logger).Act(bot, table);
                        _logger.LogInformation($"BotService. Act. action: {JsonSerializer.Serialize(action)}");
                        break;
                }
            
                return action;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }
    }
}