using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Helpers.Present;

namespace PokerHand.BusinessLogic.Services
{
    public class PresentService : IPresentService
    {
        private readonly ITablesOnline _allTables;

        public PresentService(ITablesOnline allTables)
        {
            _allTables = allTables;
        }

        public event Action<string> OnSendPresent;
        
        public void SendPresent(Guid senderId, Guid[] recipientIds, PresentName presentName)
        {
            foreach (var recipientId in recipientIds)
            {
                var present = new Present
                {
                    Name = presentName,
                    SenderId = senderId,
                    RecipientId = recipientId
                };

                var recipient = _allTables
                    .GetByPlayerId(recipientId)
                    .Players
                    .FirstOrDefault(p => p.Id == recipientId);
            
                recipient?.Presents.Add(present);
            
                OnSendPresent?.Invoke(JsonSerializer.Serialize(present));
            }
        }
    }
}