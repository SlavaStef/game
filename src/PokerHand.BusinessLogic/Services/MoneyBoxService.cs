using System;
using System.Threading.Tasks;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.BusinessLogic.Services
{
    public class MoneyBoxService : IMoneyBoxService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MoneyBoxService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultModel<int>> GetMoneyBoxAmount(Guid playerId)
        {
            var result = new ResultModel<int>();
            var playerExists = await _unitOfWork.Players.PlayerExistsAsync(playerId);

            if (playerExists is false)
            {
                result.IsSuccess = false;
                result.Message = "Player not found";
                return result;
            }

            var moneyBoxAmount = await _unitOfWork.Players.GetMoneyBoxAmountAsync(playerId);

            result.IsSuccess = true;
            result.Value = moneyBoxAmount;
            return result;
        }

        public async Task<ResultModel<int>> IncreaseMoneyBoxAmount(Guid playerId, int amount)
        {
            var result = new ResultModel<int>();
            var playerExists = await _unitOfWork.Players.PlayerExistsAsync(playerId);

            if (playerExists is false)
            {
                result.IsSuccess = false;
                result.Message = "Player not found";
                return result;
            }

            var newMoneyBoxAmount = await _unitOfWork.Players.IncreaseMoneyBoxAmountAsync(playerId, 5);
           
            result.IsSuccess = true;
            result.Value = newMoneyBoxAmount;
            return result;
        }

        public async Task<ResultModel<bool>> OpenMoneyBox(Guid playerId)
        {
            var result = new ResultModel<bool>();
            var playerExists = await _unitOfWork.Players.PlayerExistsAsync(playerId);

            if (playerExists is false)
            {
                result.IsSuccess = false;
                result.Message = "Player not found";
                return result;
            }

            await _unitOfWork.Players.OpenMoneyBoxAsync(playerId);

            result.IsSuccess = true;
            result.Value = true;
            return result;
        }
    }
}