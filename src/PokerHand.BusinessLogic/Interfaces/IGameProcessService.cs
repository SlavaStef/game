using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.GameProcess;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IGameProcessService
    {
        event Action<Table> OnPrepareForGame;
        event Action<Table, List<Card>> OnDealCommunityCards;
        event Action<Player> ReceivePlayerDto;
        event Action<Table, string> ReceiveWinners;
        event Action<Table, string> ReceiveUpdatedPot;
        event Action<Table, PlayerAction> ReceivePlayerAction;
        event Action<Table, string> ReceiveCurrentPlayerIdInWagering;
        event Action<Player> OnLackOfStackMoney;
        event Action<Table> ReceiveTableState;
        event Action<Table, string> SmallBlindBetEvent;
        event Action<Table, string> BigBlindBetEvent;
        event Action<Table, string> NewPlayerBetEvent;
        event Action<Table> EndSitAndGoGameFirstPlace;
        event Action<Player, string> EndSitAndGoGame;
        event Action<string, string> RemoveFromGroupAsync;
        event Action<Table> OnGameEnd;
        event Action<string, string> PlayerDisconnected;

        Task StartRound(Guid tableId);
    }
}