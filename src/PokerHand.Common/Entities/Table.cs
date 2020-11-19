using System;
using System.Collections.Generic;

namespace PokerHand.Common.Entities
{
    public class Table
    {
        public Table(int maxPlayers)
        {
            Id = Guid.NewGuid();
            TimeCreated = DateTime.Now;
            MaxPlayers = maxPlayers;
            Deck = new Deck();
            Players = new List<Player>();
            Pot = 0; 
            CommunityCards = new List<Card>();
            Winner = null;
        }
        
        public Guid Id { get; set; }
        public DateTime TimeCreated { get; set; }

        public int MaxPlayers { get; set; }
        public Deck Deck { get; set; }
        public List<Player> Players { get; set; }
        public int Pot { get; set; }
        public List<Card> CommunityCards { get; set; }
        public Player Winner { get; set; }
    }
}