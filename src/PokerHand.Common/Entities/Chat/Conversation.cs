using System;
using System.Collections.Generic;

namespace PokerHand.Common.Entities.Chat
{
    public class Conversation
    {
        public Guid Id { get; set; }

        public List<Message> Messages { get; set; }
        
        public Guid FirstPlayerId { get; set; }
        public Guid SecondPlayerId { get; set; }

        public virtual Player FirstPlayer { get; set; }
        public virtual Player SecondPlayer { get; set; }
    }
}