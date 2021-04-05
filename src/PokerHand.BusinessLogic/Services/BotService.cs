using System;
using System.Text.Json;
using Bogus;
using PokerHand.BusinessLogic.Helpers.BotLogic;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Player;
using PokerHand.Common.Helpers.Table;
using Serilog;

namespace PokerHand.BusinessLogic.Services
{
    public class BotService : IBotService
    {
        private readonly ICardEvaluationService _cardEvaluationService;
        private static readonly Random Random = new();

        public BotService(ICardEvaluationService cardEvaluationService)
        {
            _cardEvaluationService = cardEvaluationService;
        }

        public Player Create(Table table, BotComplexity complexity)
        {
            var tableName = table.Title.ToString();
            
            var minBuyIn = TableOptions.Tables[tableName]["MinBuyIn"];
            var maxBuyIn = TableOptions.Tables[tableName]["MaxBuyIn"];
            
            var fakeUser = new Faker<Player>()
                .RuleFor(o => o.RegistrationDate, f => f.Date.Past(0, DateTime.Now))
                .RuleFor(o => o.UserName, f => f.Internet.UserName())
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
                
                Country = (PokerHand.Common.Helpers.Player.CountryCode)Random.Next(1, 31),
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
                Log.Information("BotService. Act. Start");
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
                        Log.Information("BotService. Act. Inside Hard logic");
                        action = new HardBotLogic(_cardEvaluationService, new Random()).Act(bot, table);
                        Log.Information($"BotService. Act. action: {JsonSerializer.Serialize(action)}");
                        break;
                }
            
                return action;
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}");
                Log.Error($"{e.StackTrace}");
                throw;
            }
        }
    }
}