using System.Threading.Tasks;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task SendQuickMessage(string messageTypeJson, string messageJson, string tableIdString, string senderIndexJson)
        {
            await Clients.Group(tableIdString)
                .ReceiveQuickMessage(messageTypeJson, messageJson, senderIndexJson);
        }
    }
}