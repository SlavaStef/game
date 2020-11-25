using System;
using System.Collections.Generic;

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
            Winner = null;
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
        public List<Player> ActivePlayers { get; set; }
        public List<Card> CommunityCards { get; set; }
        public Player CurrentPlayer { get; set; }
        public int CurrentMaxBet { get; set; }
        public int Pot { get; set; }
        public Player Winner { get; set; }
       
        

        /// CurrentRound
        /// List<Player> players
        /// int maxBet
        /// 
        /// IF FOLD -> CurrentRound.Players.Remove(currentPlayer)
        /// 
        /// IF CALL -> 1) Players.First(...).CurrentBet = CurrentMaxBet 
        ///  
        /// IF RAISE(int amount) -> player.bet = maxBet = amount
    }
}

//active players - Any players still involved in the pot