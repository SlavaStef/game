using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.Common.Dto;
using PokerHand.Common.Helpers.QuickChat;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task SendQuickMessage(string messageJson, string tableIdString, string senderIndexJson)
        {
            await Clients.Group(tableIdString)
                .ReceiveQuickMessage(JsonSerializer.Serialize(new QuickMessageDto
                {
                    Message = JsonSerializer.Deserialize<QuickMessage>(messageJson),
                    SenderIndex = JsonSerializer.Deserialize<int>(senderIndexJson)
                }));
        }
    }
}