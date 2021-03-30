﻿using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IBotService
    {
        Player Create(Table table, BotComplexity complexity);
        PlayerAction Act(Player bot, Table table);
    }
}