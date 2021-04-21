using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Present;
using Serilog;

namespace PokerHand.BusinessLogic.Services
{
    public class PresentService : IPresentService
    {
        private readonly ITablesOnline _allTables;
        private readonly IPlayerService _playerService;

        public PresentService(ITablesOnline allTables, IPlayerService playerService)
        {
            _allTables = allTables;
            _playerService = playerService;
        }

        public event Action<string> OnSendPresent;
        public event Action<string> OnSendPresentError;
        public event Action<string> SendTotalMoneyAmount;


        public List<PresentInfoDto> GetAllPresentsInfo()
        {
            var allPresentsInfo = new List<PresentInfoDto>();

            foreach (var (name, price) in PresentOptions.Presents)
            {
                allPresentsInfo.Add(new PresentInfoDto
                {
                    Name = name,
                    Price = price
                });
            }

            return allPresentsInfo;
        }

        public async Task SendPresent(Guid senderId, List<Guid> recipientsIds, PresentName presentName)
        {
            var table = _allTables.GetByPlayerId(senderId);
            
            var present = new Present
            {
                Name = presentName,
                SenderIndexNumber = table.Players.First(p => p.Id == senderId).IndexNumber,
                RecipientsIndexNumbers = new List<int>()
            };
            
            var totalPresentPrice = PresentOptions.Presents[presentName] * recipientsIds.Count;
            
            await SendErrorIfNotEnoughMoney(senderId, totalPresentPrice);

            foreach (var recipientId in recipientsIds)
            {
                var player = table
                    .Players
                    .FirstOrDefault(p => p.Id == recipientId);

                if (player is null)
                {
                    OnSendPresentError?.Invoke(JsonSerializer.Serialize(SendPresentErrors.PlayerNotFound));
                    return;
                }
                
                present.RecipientsIndexNumbers.Add(player.IndexNumber);
                
                player.PresentName = present.Name;
            }
            
            OnSendPresent?.Invoke(JsonSerializer.Serialize(present));
            
            Log.Information($"SendPresent. Before GetFromTotalMoney: {await _playerService.GetTotalMoney(senderId)}");
            
            var isOk = await _playerService.GetFromTotalMoney(senderId, totalPresentPrice);
            
            Log.Information($"isOk: {isOk}");
            Log.Information($"SendPresent. After GetFromTotalMoney: {await _playerService.GetTotalMoney(senderId)}");

            var newSenderTotalMoney = await _playerService.GetTotalMoney(senderId);
            SendTotalMoneyAmount?.Invoke(JsonSerializer.Serialize(newSenderTotalMoney));
        }

        #region Helpers

        private async Task SendErrorIfNotEnoughMoney(Guid senderId, int totalPresentPrice)
        {
            var senderTotalMoney = await _playerService.GetTotalMoney(senderId);

            if (totalPresentPrice > senderTotalMoney)
                OnSendPresentError?.Invoke(JsonSerializer.Serialize(SendPresentErrors.NotEnoughMoney));
        }

        #endregion
    }
}