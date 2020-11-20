using System;
using System.Collections.Generic;
using PokerHand.Common.Entities;

namespace PokerHand.Common.Dto
{
    // DTO to send to each player
    public class TableDto
    {
        public Guid Id { get; set; }
        public bool IsInGame { get; set; }
        public List<PlayerDto> Players { get; set; }
        public int Pot { get; set; }
        public List<Card> CommunityCards { get; set; }
    }
}