using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IBotService
    {
        Bot Create(Table table, BotComplexity complexity);
        PlayerAction Act(Bot bot, Table table);
    }
}