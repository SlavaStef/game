using System;

namespace PokerHand.Common.Dto.Chat
{
    public class PublicMessageDto
    {
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime SendTime { get; set; }
    }
}