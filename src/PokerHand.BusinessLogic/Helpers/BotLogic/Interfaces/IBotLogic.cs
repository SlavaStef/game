using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Helpers.BotLogic.Interfaces
{
    public interface IBotLogic
    {
        PlayerAction Act(Player bot, Table table);
    }
}