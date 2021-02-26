using System;
using PokerHand.BusinessLogic.Helpers.BotLogic;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.Player;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Services
{
    public class BotService : IBotService
    {
        private static Random _random = new();

        public Player Create(Table table, BotComplexity complexity)
        {
            var tableName = table.Title.ToString();
            
            var minBuyIn = TableOptions.Tables[tableName]["MinBuyIn"];
            var maxBuyIn = TableOptions.Tables[tableName]["MaxBuyIn"];
            
            return new Player
            {
                Type = PlayerType.Computer,
                Complexity = complexity,
                
                TotalMoney = _random.Next(maxBuyIn, maxBuyIn * 2),
                StackMoney = _random.Next(minBuyIn + minBuyIn / 10, maxBuyIn),
                UserName = "TestBot " + _random.Next(1000, 10000),
                ConnectionId = _random.Next(10000, 100000).ToString(),
                IndexNumber = table.Players.Count,
                IsAutoTop = true,
                IsReady = true
            };
        }
            
        public PlayerAction Act(Player bot, Table table)
        {
            var action = new PlayerAction();
            
            switch (bot.Complexity)
            {
                case BotComplexity.Simple:
                    action = new SimpleBotLogic().Act(bot, table);
                    break;
                case BotComplexity.Medium:
                    action = new MediumBotLogic().Act(bot, table);
                    break;
                case BotComplexity.Hard:
                    action = new HardBotLogic().Act(bot, table);
                    break;
            }
            
            return action;
        }
    }
}