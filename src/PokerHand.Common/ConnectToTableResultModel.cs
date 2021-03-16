using PokerHand.Common.Dto;

namespace PokerHand.Common
{
    public class ConnectToTableResultModel
    {
        public TableDto TableDto { get; set; }
        public PlayerDto PlayerDto { get; set; }
        public bool IsNewTable { get; set; }
    }
}