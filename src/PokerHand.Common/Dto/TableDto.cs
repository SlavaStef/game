using System;
using System.Collections.Generic;
using PokerHand.Common.Entities;

namespace PokerHand.Common.Dto
{
    // DTO to send to each player
    public class TableDto
    {
        public Guid Id { get; set; }
        public DateTime TimeCreated { get; set; }
        public bool IsInGame { get; set; }
        public int MaxPlayers { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }
        
        public Deck Deck { get; set; }
        public List<Player> Players { get; set; }
        
        //Current round
        public List<Player> ActivePlayers { get; set; }
        public List<Card> CommunityCards { get; set; }
        public Player CurrentPlayer { get; set; }
        public int CurrentMaxBet { get; set; }
        public int Pot { get; set; }
        public Player Winner { get; set; }
    }
}