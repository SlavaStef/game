using System;

namespace PokerHand.Common.Dto.Chat
{
    public class MessageDto
    {
        public Guid SenderId { get; set; }
        public string Text { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}