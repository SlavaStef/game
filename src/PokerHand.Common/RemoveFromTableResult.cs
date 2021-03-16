using PokerHand.Common.Dto;

namespace PokerHand.Common
{
    public class RemoveFromTableResult
    {
        public bool WasPlayerRemoved { get; set; }
        public bool WasTableRemoved { get; set; }
        public TableDto TableDto { get; set; }
    }
}