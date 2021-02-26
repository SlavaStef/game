using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Bot;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IBotService
    {
        Player Create(Table table, BotComplexity complexity);
        PlayerAction Act(Player bot, Table table);
    }
}