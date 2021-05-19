using System;

namespace PokerHand.Common.ViewModels.Profile
{
    public class AddMoneyVM
    {
        public Guid PlayerId { get; set; }
        public int Amount { get; set; }
    }
}