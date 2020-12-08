using System;
using System.Collections.Generic;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class Table
    {
        public Table()
        {

        }

        public Table(int maxPlayers)
        {
            Id = Guid.NewGuid();
            TimeCreated = DateTime.Now;
            IsInGame = false;
            MaxPlayers = maxPlayers;
            SmallBlind = 25;
            BigBlind = 50;
            Deck = new Deck();
            Players = new List<Player>();
            Pot = 0;
            CommunityCards = new List<Card>();
            Winners = null;
            DealerIndex = -1;
            SmallBlindIndex = -1;
            BigBlindIndex = -1;
        }

        public Guid Id { get; set; }
        public DateTime TimeCreated { get; set; }
        public bool IsInGame { get; set; }
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
    }
}