using System;

namespace PokerHand.Common.Entities.Chat
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeCreated { get; set; }
        
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }

        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }
    }
}