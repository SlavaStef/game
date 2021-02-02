using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private ApplicationContext _context;
        private bool disposed = false;
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        
        public IPlayerRepository Players { get; private set; }
        public IConversationRepository Conversations { get; private set; }
        public IMessageRepository Messages { get; private set; }

        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
            
            Players = new PlayerRepository(_context);
            Conversations = new ConversationRepository(_context);
            Messages = new MessageRepository(_context);
            
        }
        
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void DisableAutoDetectChanges()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
            }

            disposed = true;
        }
    }
}