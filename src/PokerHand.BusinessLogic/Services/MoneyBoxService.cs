using System;
using System.Threading.Tasks;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.BusinessLogic.Services
{
    public class MoneyBoxService : IMoneyBoxService
    {
        private const int MaxAmount = 2_000_000;
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

        public async Task<ResultModel<int>> IncreaseMoneyBoxAmount(Guid playerId, int amountToAdd)
        {
            var result = new ResultModel<int>();
            var playerExists = await _unitOfWork.Players.PlayerExistsAsync(playerId);

            if (playerExists is false)
            {
                result.IsSuccess = false;
                result.Message = "Player not found";
                return result;
            }

            var currentMoneyBoxAmount = await _unitOfWork.Players.GetMoneyBoxAmountAsync(playerId);

            if (currentMoneyBoxAmount >= MaxAmount)
            {
                result.IsSuccess = false;
                result.Message = "MoneyBox is full";
                result.Value = currentMoneyBoxAmount;

                return result;
            }
            
            int newMoneyBoxAmount;

            if (currentMoneyBoxAmount + amountToAdd >= MaxAmount)
                newMoneyBoxAmount =
                    await _unitOfWork.Players.IncreaseMoneyBoxAmountAsync(playerId, MaxAmount - amountToAdd);
            else
                newMoneyBoxAmount = await _unitOfWork.Players.IncreaseMoneyBoxAmountAsync(playerId, amountToAdd);

            result.IsSuccess = true;
            result.Value = newMoneyBoxAmount;
            return result;
        }

        public async Task<ResultModel<int>> OpenMoneyBox(Guid playerId)
        {
            var result = new ResultModel<int>();
            var playerExists = await _unitOfWork.Players.PlayerExistsAsync(playerId);

            if (playerExists is false)
            {
                result.IsSuccess = false;
                result.Message = "Player not found";
                return result;
            }

            var newTotalMoneyAmount = await _unitOfWork.Players.OpenMoneyBoxAsync(playerId);

            result.IsSuccess = true;
            result.Value = newTotalMoneyAmount;
            return result;
        }
    }
}