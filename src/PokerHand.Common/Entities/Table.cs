using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public sealed class Table : IDisposable
    {
        private bool _disposed = false;
        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);
        //public Mutex Mutex { get; set; }
        public AutoResetEvent WaitForPlayerBet { get; set; }
        
        public Table() { }

        public Table(TableTitle title)
        {
            Id = Guid.NewGuid();
            TimeCreated = DateTime.Now;
            Title = title;
            SetOptions(title);

            Deck = new Deck(Type);
            Players = new List<Player>();
            Pot = 0;
            CommunityCards = new List<Card>();
            Winners = null;
            DealerIndex = -1;
            SmallBlindIndex = -1;
            BigBlindIndex = -1;

            WaitForPlayerBet = new AutoResetEvent(false);
            //Mutex = new Mutex(true);
        }

        public Guid Id { get; }
        public DateTime TimeCreated { get; set; }
        public TableTitle Title { get; set; }
        public TableType Type { get; set; }
        public int MaxPlayers { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }

        public Deck Deck { get; set; }
        public List<Player> Players { get; set; }

        //Current round
        public RoundStageType CurrentStage { get; set; }
        public List<Player> ActivePlayers { get; set; }
        public List<Card> CommunityCards { get; set; }
        public Player CurrentPlayer { get; set; }
        public int CurrentMaxBet { get; set; }
        public int Pot { get; set; }
        public int DealerIndex { get; set; }
        public int SmallBlindIndex { get; set; }
        public int BigBlindIndex { get; set; }
        public List<Player> Winners { get; set; }

        private void SetOptions(TableTitle title)
        {
            switch (title)
            {
                case TableTitle.TropicalHouse:
                    Type = (TableType)TableOptions.TropicalHouseOptions["Type"];
                    MaxPlayers = TableOptions.TropicalHouseOptions["MaxPlayers"];
                    BigBlind = TableOptions.TropicalHouseOptions["BigBlind"];
                    SmallBlind = TableOptions.TropicalHouseOptions["SmallBlind"];
                    break;
            }
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
            {
                _handle.Dispose();
            }

            _disposed = true;
        }
    }
}