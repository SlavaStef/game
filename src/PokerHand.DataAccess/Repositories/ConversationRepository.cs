using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PokerHand.Common.Entities.Chat;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.DataAccess.Repositories
{
    public class ConversationRepository : Repository<Conversation>, IConversationRepository
    {
        public ConversationRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<Conversation> FindConversationAsync(Guid senderId, Guid recipientId)
        {
            return await _context.Conversations
                .Where(c => c.FirstPlayerId == senderId || c.FirstPlayerId == recipientId)
                .FirstOrDefaultAsync(c => c.SecondPlayerId == senderId || c.SecondPlayerId == recipientId);
        }
    }
}