using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Helpers.BotLogic.Interfaces
{
    public interface IBotLogic
    {
        PlayerAction Act(Player bot, Table table);
    }
}