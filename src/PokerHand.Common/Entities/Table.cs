using System;
using System.Collections.Generic;

namespace PokerHand.Common.Entities
{
    public class Table
    {
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