using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Player;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Services
{
    public class TableService : ITableService
    {
        private readonly ITablesOnline _allTables;
        private readonly UserManager<Player> _userManager;
        private readonly IPlayerService _playerService;
        private readonly IDeckService _deckService;
        private readonly ILogger<TableService> _logger;
        private readonly IMapper _mapper;
        
        public event Action<Table, string> ReceivePlayerAction;
        public event Action<Player, string> EndSitAndGoGame;
        public event Action<string, string> PlayerDisconnected;
        public event Action<string, string> RemoveFromGroupAsync;

        public TableService(
            UserManager<Player> userManager,
            ILogger<TableService> logger, 
            IMapper mapper, 
            IPlayerService playerService, 
            ITablesOnline allTables, 
            IDeckService deckService)
        {
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            _playerService = playerService;
            _allTables = allTables;
            _deckService = deckService;
        }

        public TableInfoDto GetTableInfo(string tableName)
        {
            var tableInfoDto = new TableInfoDto
            {
                Title = tableName,
                TableType = TableOptions.Tables[tableName]["TableType"],
                Experience = TableOptions.Tables[tableName]["Experience"],
                SmallBlind = TableOptions.Tables[tableName]["SmallBlind"],
                BigBlind = TableOptions.Tables[tableName]["BigBlind"],
                MinBuyIn = TableOptions.Tables[tableName]["MinBuyIn"],
                MaxBuyIn = TableOptions.Tables[tableName]["MaxBuyIn"],
                MaxPlayers = TableOptions.Tables[tableName]["MaxPlayers"]
            };
            
            if (TableOptions.Tables[tableName]["TableType"] == (int)TableType.SitAndGo)
            {
                tableInfoDto.InitialStack = TableOptions.Tables[tableName]["InitialStack"];
                tableInfoDto.FirstPlacePrize = TableOptions.Tables[tableName]["FirstPlacePrize"];
                tableInfoDto.SecondPlacePrize = TableOptions.Tables[tableName]["SecondPlacePrize"];
            }

            return tableInfoDto;
        }

        public List<TableInfoDto> GetAllTablesInfo()
        {
            var allTablesInfo = new List<TableInfoDto>();

            foreach (var table in TableOptions.Tables)
            {
                var tableInfoDto = new TableInfoDto
                {
                    Title = table.Key,
                    TableType = table.Value["TableType"],
                    Experience = table.Value["Experience"],
                    SmallBlind = table.Value["SmallBlind"],
                    BigBlind = table.Value["BigBlind"],
                    MinBuyIn = table.Value["MinBuyIn"],
                    MaxBuyIn = table.Value["MaxBuyIn"],
                    MaxPlayers = table.Value["MaxPlayers"]
                };

                if (table.Value["TableType"] is (int)TableType.SitAndGo)
                {
                    tableInfoDto.InitialStack = table.Value["InitialStack"];
                    tableInfoDto.FirstPlacePrize = table.Value["FirstPlacePrize"];
                    tableInfoDto.SecondPlacePrize = table.Value["SecondPlacePrize"];
                }
                
                allTablesInfo.Add(tableInfoDto);
            }

            return allTablesInfo;
        }

        public async Task<ResultModel<ConnectToTableResult>> AddPlayerToTable(TableConnectionOptions options)
        {
            try
            {
                _logger.LogInformation($"{JsonSerializer.Serialize(options)}");
                var result = new ResultModel<ConnectToTableResult>(new ConnectToTableResult());
            
                var table = GetFreeTable(options.TableTitle, options.CurrentTableId);
 
                if (table is null)
                {
                    table = CreateNewTable(options.TableTitle);
                    result.Value.IsNewTable = true;
                }

                var player = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == options.PlayerId);
                if (player is null)
                {
                    result.IsSuccess = false;
                    result.Message = "Player not found";
                    return result;
                }
            
                player.ConnectionId = options.PlayerConnectionId;
                player.IndexNumber = result.Value.IsNewTable ? 0 : GetFreeSeatIndex(table);

                if (table.Type is TableType.SitAndGo)
                    await ConfigureSitAndGo(options, player);
                else
                {
                    player.IsAutoTop = options.IsAutoTop;
                    player.CurrentBuyIn = options.BuyInAmount;
                
                    if (player.TotalMoney >= options.BuyInAmount)
                    {
                        await _playerService.GetFromTotalMoney(player.Id, options.BuyInAmount);
                        player.StackMoney = options.BuyInAmount;
                    }

                    if (player.StackMoney is 0)
                    {
                        result.Value.TableDto = null;
                        result.Value.PlayerDto = _mapper.Map<PlayerDto>(player);
                        result.Value.IsNewTable = false;
                    }
                }

                table.Players.Add(player);
                table.Players = table.Players.OrderBy(p => p.IndexNumber).ToList();

                result.IsSuccess = true;
                result.Value.PlayerDto = _mapper.Map<PlayerDto>(player);
                result.Value.TableDto = _mapper.Map<TableDto>(table);
            
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
            
        }

        public async Task<ResultModel<RemoveFromTableResult>> RemovePlayerFromTable(Guid tableId, Guid playerId)
        {
            var table = _allTables.GetById(tableId);
            if (table is null)
                return new ResultModel<RemoveFromTableResult> {IsSuccess = false, Message = "Table is null"};
            
            var player = table.Players.FirstOrDefault(p => p.Id == playerId);
            if (player is null)
                return new ResultModel<RemoveFromTableResult> {IsSuccess = false, Message = "Player is null"};
            
            _logger.LogInformation($"RemovePlayerFromTable. count: {table.Players.Count(p => p.Type is PlayerType.Human)}");

            foreach (var tablePlayer in table.Players)
            {
                _logger.LogInformation($"{tablePlayer.UserName} : {JsonSerializer.Serialize(tablePlayer.Type)}");
            }
            if (table.Players.Count(p => p.Type is not PlayerType.Computer) is 1)
            {
                _logger.LogInformation("RemovePlayerFromTable. One human player");
                await RemovePlayer(player, table);
                _logger.LogInformation("RemovePlayerFromTable. One human player. player removed");

                _allTables.Remove(table.Id);
                
                _logger.LogInformation("RemovePlayerFromTable. One human player. table removed");

                return new ResultModel<RemoveFromTableResult>
                {
                    IsSuccess = true,
                    Value = new RemoveFromTableResult {WasPlayerRemoved = true, WasTableRemoved = true}
                };
            }

            if (table.CurrentPlayer?.Id == player.Id)
            {
                _logger.LogInformation("RemovePlayerFromTable. second If");
                await RemoveCurrentPlayer(player, table);

                return new ResultModel<RemoveFromTableResult>
                {
                    IsSuccess = true, 
                    Value = new RemoveFromTableResult
                        {WasPlayerRemoved = true, WasTableRemoved = false, TableDto = _mapper.Map<TableDto>(table)}
                };
            }
            
            _logger.LogInformation("RemovePlayerFromTable. just remove");
            await RemovePlayer(player, table);

            return new ResultModel<RemoveFromTableResult>
            {
                IsSuccess = true, 
                Value = new RemoveFromTableResult
                    {WasPlayerRemoved = true, WasTableRemoved = false, TableDto = _mapper.Map<TableDto>(table)}
            };
        }

        #region Helpers
        private Table GetFreeTable(TableTitle tableTitle, Guid? currentTableId)
        {
            try
            {
                if (tableTitle is TableTitle.RivieraHotel ||
                    tableTitle is TableTitle.CityDreamsResort ||
                    tableTitle is TableTitle.HeritageBank)
                {
                    return _allTables
                        .GetManyByTitle(tableTitle)
                        .FirstOrDefault(t => t.CurrentStage is RoundStageType.NotStarted);
                }
            
                return _allTables
                    .GetManyByTitle(tableTitle)
                    .FirstOrDefault(t => t.Players.Count < t.MaxPlayers && t.Id != currentTableId);

            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
            
        }

        private Table CreateNewTable(TableTitle tableTitle)
        {
            var table = new Table();
            
            table.Id = Guid.NewGuid();
            table.WaitForPlayerBet = new AutoResetEvent(false);
            table.Title = tableTitle;
            table.CurrentStage = RoundStageType.NotStarted;
            SetTableOptions(table);

            table.Deck = _deckService.GetNewDeck(table.Type);
            table.Players = new List<Player>(table.MaxPlayers);
            table.ActivePlayers = new List<Player>(table.MaxPlayers);
            table.Pot = new Pot();
            table.CommunityCards = new List<Card>(5);
            table.DealerIndex = -1;
            table.SmallBlindIndex = -1;
            table.BigBlindIndex = -1;
            table.SitAndGoRoundCounter = 0;
            table.SidePots = new List<SidePot>(table.MaxPlayers);
            table.SitAndGoRoundCounter = 0;
            
            _allTables.Add(table);
            
            return table;
        }
        
        private static void SetTableOptions(Table table)
        {
            var tableName = Enum.GetName(typeof(TableTitle), (int) table.Title);

            if (tableName is null)
                throw new Exception();
            
            table.Type = (TableType)TableOptions.Tables[tableName]["TableType"];
            table.SmallBlind = TableOptions.Tables[tableName]["SmallBlind"];
            table.BigBlind = TableOptions.Tables[tableName]["BigBlind"];
            table.MaxPlayers = TableOptions.Tables[tableName]["MaxPlayers"];
            
            table.MinPlayersToStart = table.Type is TableType.SitAndGo ? 5 : 2;
        }
        
        private async Task ConfigureSitAndGo(TableConnectionOptions connectionOptions, Player player)
        {
            player.IsAutoTop = false;
            player.CurrentBuyIn = TableOptions.Tables[connectionOptions.TableTitle.ToString()]["MinBuyIn"];

            await _playerService.GetFromTotalMoney(player.Id, player.CurrentBuyIn);
            player.StackMoney = TableOptions.Tables[connectionOptions.TableTitle.ToString()]["InitialStack"];
        }

        private static int GetFreeSeatIndex(Table table)
        {
            // table seats counter starts from 0
            var seatIndex = -1;

            for (var i = 0; i < table.MaxPlayers; i++)
            {
                if (table.Players.Any(player => player.IndexNumber == i)) 
                    continue;
                
                seatIndex = i;
                break;
            }

            return seatIndex;
        }
        
        // RemovePlayerFromTable helpers
        private async Task RemovePlayer(Player player, Table table)
        {
            await _playerService.IncreaseNumberOfPlayedGamesAsync(player.Id, false);

            if (player.CurrentBet != 0)
                table.Pot.TotalAmount += player.CurrentBet;

            if (player.StackMoney > 0)
                await _playerService.AddTotalMoney(player.Id, player.StackMoney);
            
            if (table.Type is TableType.SitAndGo)
            {
                var playerPlace = table.Players.Count;
                _logger.LogInformation($"RemoveCurrentPlayer. playerPlace: {playerPlace}");
                
                EndSitAndGoGame?.Invoke(player, playerPlace.ToString());
            }

            if (table.ActivePlayers.Contains(player))
                table.ActivePlayers.Remove(player);

            table.Players.Remove(player);
        }

        private async Task RemoveCurrentPlayer(Player player, Table table)
        {
            player.CurrentAction = new PlayerAction
            {
                ActionType = PlayerActionType.Fold,
                PlayerIndexNumber = player.IndexNumber
            };

            ReceivePlayerAction?.Invoke(table, JsonSerializer.Serialize(table.CurrentPlayer.CurrentAction));

            await RemovePlayer(player, table);

            RemoveFromGroupAsync?.Invoke(player.ConnectionId, table.Id.ToString());
            PlayerDisconnected?.Invoke(table.Id.ToString(), JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            
            table.WaitForPlayerBet.Set();
        }
        #endregion
    }
}