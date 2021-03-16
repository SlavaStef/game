using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Table;
using Table = Microsoft.EntityFrameworkCore.Metadata.Internal.Table;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface ITableService
    {
        public event Action<Common.Entities.Table, string> ReceivePlayerAction;
        public event Action<Player, string> EndSitAndGoGame;
        public event Action<string, string> PlayerDisconnected;
        public event Action<string, string> RemoveFromGroupAsync;
        
        TableInfoDto GetTableInfo(string tableName);
        List<TableInfoDto> GetAllTablesInfo();
        Task<ConnectToTableResultModel> AddPlayerToTable(TableConnectionOptions options);
        Task<ResultModel<RemoveFromTableResult>> RemovePlayerFromTable(Guid tableId, Guid playerId);
    }
}