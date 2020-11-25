using System.Threading;

namespace PokerHand.Common.Helpers
{
    public static class Waiter
    {
        public static AutoResetEvent WaitForPlayerBet = new AutoResetEvent(true);
    }
}