using System;
using System.Threading.Tasks;
using PokerHand.Common;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IMoneyBoxService
    {
        Task<ResultModel<int>> GetMoneyBoxAmount(Guid playerId);
        Task<ResultModel<int>> IncreaseMoneyBoxAmount(Guid playerId, int amount);
        Task<ResultModel<bool>> OpenMoneyBox(Guid playerId);
    }
}