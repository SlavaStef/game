using PokerHand.Common.Entities.Chat;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.DataAccess.Repositories
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationContext context) : base(context)
        {
        }
    }
}