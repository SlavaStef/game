﻿using System;
using System.Collections.Generic;
using System.Threading;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.Common.Entities
{
    public sealed class Table
    {   
        public Guid Id { get; set; }
        public AutoResetEvent WaitForPlayerBet { get; set; }
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
        public Pot Pot { get; set; }
        public int DealerIndex { get; set; }
        public int SmallBlindIndex { get; set; }
        public int BigBlindIndex { get; set; }
        public List<SidePot> SidePots { get; set; }
        public int SitAndGoRoundCounter { get; set; }
    }
}