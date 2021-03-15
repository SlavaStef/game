using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.GameProcess;

namespace PokerHand.BusinessLogic.Helpers.BotLogic.Interfaces
{
    public interface IBotLogic
    {
        PlayerAction Act(Player bot, Table table);
    }
}