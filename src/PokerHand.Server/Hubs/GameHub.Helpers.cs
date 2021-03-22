using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PokerHand.Common;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        private async Task AddPlayerToTableGroup(ResultModel<ConnectToTableResult> connectionResult)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, connectionResult.Value.TableDto.Id.ToString());
            await Clients.Caller
                .ReceivePlayerDto(JsonSerializer.Serialize(connectionResult.Value.PlayerDto));
            await Clients.Group(connectionResult.Value.TableDto.Id.ToString())
                .ReceiveTableState(JsonSerializer.Serialize(connectionResult.Value.TableDto));
        }
        
        private void StartGameOnNewTable(Guid id) =>
            new Thread(() => _gameProcessService.StartRound(id)).Start();
    }
}