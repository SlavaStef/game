using System;
using System.Collections.Generic;

namespace PokerHand.Common
{
    public class PlayersOnline
    {
        public Dictionary<Guid, string> Players { get; set; }

        public PlayersOnline()
        {
            Players = new Dictionary<Guid, string>();
        }
    }
}