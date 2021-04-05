using System;

namespace PokerHand.Common.Helpers.Present
{
    public class Present
    {
        public PresentName Name { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
    }
}