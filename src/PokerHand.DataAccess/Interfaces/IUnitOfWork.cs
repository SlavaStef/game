using System.Threading.Tasks;

namespace PokerHand.DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        IPlayerRepository Players { get; }
        IConversationRepository Conversations { get; }
        IMessageRepository Messages { get; }
        
        void DisableAutoDetectChanges();
        Task<int> CompleteAsync();
    }
}