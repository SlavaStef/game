using System.Threading.Tasks;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task SendQuickMessage(string quickMessageJson, string tableIdString)
        {
            await Clients.OthersInGroup(tableIdString).ReceiveQuickMessage(quickMessageJson);
        }
    }
}