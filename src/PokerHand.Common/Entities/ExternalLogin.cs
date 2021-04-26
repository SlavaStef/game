using System;
using PokerHand.Common.Helpers.Authorization;

namespace PokerHand.Common.Entities
{
    public class ExternalLogin
    {
        public string ProviderKey { get; set; }
        public ExternalProviderName ProviderName { get; set; }
        
        
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }
    }
}