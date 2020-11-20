using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PokerHand.Common.Entities
{
    public class Table
    {
        public Table(int maxPlayers)
        {
            Id = Guid.NewGuid();
            TimeCreated = DateTime.Now;
            IsInGame = false;
            MaxPlayers = maxPlayers;
            Deck = new Deck();
            Players = new List<Player>();
            Pot = 0;
            CommunityCards = new List<Card>();
            Winner = null;

            for (int i = 0; i < Players.Count; i++)
                Players[i].IndexNumber = i;
            
        }
        
        public Guid Id { get; set; }
        public DateTime TimeCreated { get; set; }
        public bool IsInGame { get; set; }

        public int MaxPlayers { get; set; }
        public Deck Deck { get; set; }
        public List<Player> Players { get; set; }
        public int Pot { get; set; }
        public List<Card> CommunityCards { get; set; }
        public Player Winner { get; set; }
    }
}