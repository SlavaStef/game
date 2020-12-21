using System;
using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Dto
{
    public class TableDto
    {
        public Guid Id { get; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }
        
        public RoundStageType CurrentStage { get; set; }
        public Deck Deck { get; set; }
        public List<Player> Players { get; set; }
        public List<Player> ActivePlayers { get; set; }
        public List<Card> CommunityCards { get; set; }
        public Player CurrentPlayer { get; set; }
        public int CurrentMaxBet { get; set; }
        public int Pot { get; set; }
        public int DealerIndex { get; set; }
        public int SmallBlindIndex { get; set; }
        public int BigBlindIndex { get; set; }
        public List<Player> Winners { get; set; }
    }
}