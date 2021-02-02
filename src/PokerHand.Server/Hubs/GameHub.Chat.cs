using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PokerHand.Common.Dto;
using PokerHand.Common.Dto.Chat;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task<ConversationDto> GetConversation(string senderId, string recipientId)
        {
            var senderIdGuid = JsonSerializer.Deserialize<Guid>(senderId);
            var recipientIdGuid = JsonSerializer.Deserialize<Guid>(recipientId);
            
            var result = await _chatService.GetConversationAsync(senderIdGuid, recipientIdGuid);

            if (!result.IsSuccess)
                //TODO: send error message
                return null;

            return result.Value;
        }
        
        public async Task SendPrivateMessage(string senderId, string recipientId, string messageText)
        {
            var senderIdGuid = JsonSerializer.Deserialize<Guid>(senderId);
            var recipientIdGuid = JsonSerializer.Deserialize<Guid>(recipientId);
            var messageTextString = JsonSerializer.Deserialize<string>(messageText);

            var privateMessageDto = new PrivateMessageDto
            {
                SenderId = senderIdGuid,
                Text = messageTextString,
                SendTime = DateTime.Now
            };
            
            // If recipient is online
            if (_allPlayers.ContainsKey(recipientIdGuid))
            {
                var recipientConnectionId = _allPlayers.First(p => p.Key == recipientIdGuid).Value;
                await Clients.Client(recipientConnectionId)
                    .ReceivePrivateMessage(JsonSerializer.Serialize(privateMessageDto));
            }
            
            await _chatService.SaveMessageAsync(senderIdGuid, recipientIdGuid, messageTextString);
            await Clients.Caller.ConfirmMessageWasSent();
        }

        public async Task SendPublicMessage(string senderUserName, string tableId, string messageText)
        {
            var tableIdGuid = JsonSerializer.Deserialize<Guid>(tableId);

            var publicMessageDto = new PublicMessageDto
            {
                UserName = JsonSerializer.Deserialize<string>(senderUserName),
                Text = JsonSerializer.Deserialize<string>(messageText),
                SendTime = DateTime.Now
            };

            await Clients.Group(tableIdGuid.ToString())
                .ReceivePublicMessage(JsonSerializer.Serialize(publicMessageDto));
        }
    }
}