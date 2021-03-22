using System;

namespace PokerHand.Common.Helpers.Table
{
    public class TableConnectionOptions
    {
        public TableTitle TableTitle { get; set; }
        public Guid PlayerId { get; set; }
        public Guid? CurrentTableId { get; set; }
        public string PlayerConnectionId { get; set; }
        public int BuyInAmount { get; set; }
        public bool IsAutoTop { get; set; }
    }
}