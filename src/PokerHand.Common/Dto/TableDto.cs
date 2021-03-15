using System;
using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.Common.Dto
{
    public class TableDto
    {
        public Guid Id { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }
        public TableTitle Title { get; set; }
        public TableType Type { get; set; }
        
        public RoundStageType CurrentStage { get; set; }
        public List<PlayerDto> Players { get; set; }
        public List<PlayerDto> ActivePlayers { get; set; }
        public List<Card> CommunityCards { get; set; }
        public Player CurrentPlayer { get; set; }
        public int CurrentMaxBet { get; set; }
        public Pot Pot { get; set; }
        public int DealerIndex { get; set; }
        public int SmallBlindIndex { get; set; }
        public int BigBlindIndex { get; set; }
    }
}