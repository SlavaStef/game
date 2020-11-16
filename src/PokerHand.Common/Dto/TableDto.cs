using System;
using System.Collections.Generic;
using PokerHand.Common.Entities;

namespace PokerHand.Common.Dto
{
    // DTO to send to each player
    public class TableDto
    {
        public Guid Id { get; set; }
        public Deck Deck { get; set; }
        public PlayerDto Player { get; set; }
        public int Pot { get; set; }
        public List<Card> TableCards { get; set; }
    }
}