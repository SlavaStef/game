using System.Threading;

namespace PokerHand.Common.Helpers
{
    public static class Waiter
    {
        public static AutoResetEvent waitForPlayerBet = new AutoResetEvent(true);
    }
}