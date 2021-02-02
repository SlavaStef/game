using System;

namespace PokerHand.Common.Dto.Chat
{
    public class PrivateMessageDto
    {
        public Guid SenderId { get; set; }
        public string Text { get; set; }
        public DateTime SendTime { get; set; }
    }
}