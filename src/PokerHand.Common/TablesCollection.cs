using System.Collections.Generic;
using PokerHand.Common.Entities;

namespace PokerHand.Common
{
    public class TablesCollection
    {
        public List<Table> Tables { get; set; }
        
        public TablesCollection()
        {
            Tables = new List<Table>();
        }
    }
}