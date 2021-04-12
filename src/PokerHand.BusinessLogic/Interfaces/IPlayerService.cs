using System;
using System.Threading.Tasks;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Helpers.Player;
using PokerHand.Common.ViewModels.Profile;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerProfileDto> CreatePlayer(string playerName, Gender gender, HandsSpriteType handsSprite,
            string ipAddress);

        Task<bool> GetFromTotalMoney(Guid playerId, int amount);
        Task<bool> AddStackMoneyFromTotalMoney(Guid tableId, Guid playerId, int requiredAmount);
        Task AddTotalMoney(Guid playerId, int amountToAdd);
        Task<int> GetTotalMoney(Guid playerId);
        void SetPlayerReady(Guid tableId, Guid playerId);
        void ChangeAutoTop(Guid tableId, Guid playerId, bool isAutoTop);
        
        Task<ResultModel<PlayerProfileDto>> GetProfile(Guid playerId);
        Task<ResultModel<PlayerProfileDto>> UpdateProfile(UpdateProfileVM viewModel);

        Task IncreaseNumberOfPlayedGamesAsync(Guid playerId, bool isWin);
        Task IncreaseNumberOfSitNGoWinsAsync(Guid playerId);
        Task ChangeBestHandTypeAsync(Guid playerId, int newHandType);
        Task ChangeBiggestWinAsync(Guid playerId, int newBiggestWin);
        Task AddExperienceAsync(Guid playerId, int experience);
    }
}