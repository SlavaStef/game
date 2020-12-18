using System.Collections.Generic;

namespace PokerHand.Common.Entities
{
    public class HandRank
    {
        public string RankName { get; set; }
        public double Rank { get; set; }
        public List<string[]> Cards { get; set; }
        //public ApplicationUser User { get; set; }
    }
}