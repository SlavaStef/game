using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Helpers.BotLogic;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.Player;
using PokerHand.Common.Helpers.Table;
using Serilog;

namespace PokerHand.BusinessLogic.Services
{
    public class BotService : IBotService
    {
        private readonly ICardEvaluationService _cardEvaluationService;
        private static readonly Random Random = new();
        private readonly ILogger<BotService> _logger;

        public BotService(ICardEvaluationService cardEvaluationService, ILogger<BotService> logger)
        {
            _cardEvaluationService = cardEvaluationService;
            _logger = logger;
        }

        public Player Create(Table table, BotComplexity complexity)
        {
            var tableName = table.Title.ToString();
            
            var minBuyIn = TableOptions.Tables[tableName]["MinBuyIn"];
            var maxBuyIn = TableOptions.Tables[tableName]["MaxBuyIn"];
            
            return new Player
            {
                Type = PlayerType.Computer,
                Complexity = complexity,
                
                TotalMoney = Random.Next(maxBuyIn, maxBuyIn * 2),
                StackMoney = Random.Next(minBuyIn + minBuyIn / 10, maxBuyIn),
                UserName = "TestBot " + Random.Next(1000, 10000),
                ConnectionId = Random.Next(10000, 100000).ToString(),
                IndexNumber = table.Players.Count,
                IsAutoTop = true,
                IsReady = true
            };
        }
            
        public PlayerAction Act(Player bot, Table table)
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
                    action = new HardBotLogic(_cardEvaluationService).Act(bot, table);
                    _logger.LogInformation($"BotService. Act. action: {JsonSerializer.Serialize(action)}");
                    break;
            }
            
            return action;
        }
    }
}