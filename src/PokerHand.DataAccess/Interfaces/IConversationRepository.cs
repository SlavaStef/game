using System;
using System.Threading.Tasks;
using PokerHand.Common.Entities.Chat;

namespace PokerHand.DataAccess.Interfaces
{
    public interface IConversationRepository : IRepository<Conversation>
    {
        Task<Conversation> FindConversationAsync(Guid senderId, Guid recipientId);
    }
}