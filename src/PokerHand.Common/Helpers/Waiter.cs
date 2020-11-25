using System.Threading;

namespace PokerHand.Common.Helpers
{
    public static class Waiter
    {
        public static readonly AutoResetEvent WaitForPlayerBet = new AutoResetEvent(false);
    }
}