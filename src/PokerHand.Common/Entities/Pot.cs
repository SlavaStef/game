using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PokerHand.Common.Entities
{
    public class Pot
    {
        public int TotalAmount { get; set; }
        public Dictionary<Guid, int> Bets { get; set; }

        // Has to be initialized when ActivePlayers have been already set
        public Pot(Table table)
        {
            Bets = new Dictionary<Guid, int>();
            TotalAmount = 0;
        }
    }
}