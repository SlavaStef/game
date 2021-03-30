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
        private readonly ApplicationContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private bool _disposed = false;
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        
        public IPlayerRepository Players { get; private set; }
        public IExternalLoginRepository ExternalLogins { get; private set; }

        public UnitOfWork(
            ApplicationContext context, 
            ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;

            Players = new PlayerRepository(_context, _logger);
            ExternalLogins = new ExternalLoginRepository(_context, _logger);
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
            if (_disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
            }

            _disposed = true;
        }
    }
}