using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.Common.Entities
{
    public sealed class Table : IDisposable
    {
        private bool _disposed = false;
        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);
        public AutoResetEvent WaitForPlayerBet { get; set; } = new(false);
        
        public Table(TableTitle title)
        {
            Id = Guid.NewGuid();
            TimeCreated = DateTime.Now;
            Title = title;
            CurrentStage = RoundStageType.NotStarted;
            SetOptions(title);

            Deck = new Deck(Type);
            Players = new List<Player>(MaxPlayers);
            ActivePlayers = new List<Player>(MaxPlayers);
            Pot = new Pot(this);
            CommunityCards = new List<Card>(5);
            Winners = new List<Player>(MaxPlayers);
            DealerIndex = -1;
            SmallBlindIndex = -1;
            BigBlindIndex = -1;

            //WaitForPlayerBet = new AutoResetEvent(false);
        }

        public Guid Id { get; }
        public DateTime TimeCreated { get; set; }
        public TableTitle Title { get; set; }
        public TableType Type { get; set; }
        public int MaxPlayers { get; set; }
        public int MinPlayersToStart { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }

        public Deck Deck { get; set; }
        public List<Player> Players { get; set; }

        //Current round
        public RoundStageType CurrentStage { get; set; }
        public List<Player> ActivePlayers { get; set; }
        public List<Card> CommunityCards { get; set; }
        public bool IsEndDueToAllIn { get; set; }
        public Player CurrentPlayer { get; set; }
        public int CurrentMaxBet { get; set; }
        public Pot Pot { get; set; } // Has to be initialized when ActivePlayers have been already set
        public int DealerIndex { get; set; }
        public int SmallBlindIndex { get; set; }
        public int BigBlindIndex { get; set; }
        public List<Player> Winners { get; set; }

        private void SetOptions(TableTitle tableTitle)
        {
            var tableName = Enum.GetName(typeof(TableTitle), (int) tableTitle);

            if (tableName is null)
                throw new Exception();
            
            Type = (TableType)TableOptions.Tables[tableName]["TableType"];
            SmallBlind = TableOptions.Tables[tableName]["SmallBlind"];
            BigBlind = TableOptions.Tables[tableName]["BigBlind"];
            MaxPlayers = TableOptions.Tables[tableName]["MaxPlayers"];
            
            MinPlayersToStart = Type == TableType.SitAndGo 
                ? 5 
                : 2;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _handle.Dispose();

            _disposed = true;
        }
    }
}