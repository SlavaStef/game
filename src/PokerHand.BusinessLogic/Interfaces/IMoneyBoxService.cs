using System;
using System.Threading.Tasks;
using PokerHand.Common;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IMoneyBoxService
    {
        Task<ResultModel<int>> GetMoneyBoxAmount(Guid playerId);
        Task<ResultModel<int>> IncreaseMoneyBoxAmount(Guid playerId, int amountToAdd);
        Task<ResultModel<int>> OpenMoneyBox(Guid playerId);
    }
}