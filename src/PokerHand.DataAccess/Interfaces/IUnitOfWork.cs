using System.Threading.Tasks;

namespace PokerHand.DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        IPlayerRepository Players { get; }
        IExternalLoginRepository ExternalLogins { get; }
        
        void DisableAutoDetectChanges();
        Task<int> CompleteAsync();
    }
}