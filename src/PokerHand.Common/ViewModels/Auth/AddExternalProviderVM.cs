using System;
using PokerHand.Common.Helpers.Authorization;

namespace PokerHand.Common.ViewModels.Auth
{
    public class AddExternalProviderVM
    {
        public Guid PlayerId { get; set; }
        public ExternalProviderName ProviderName { get; set; }
        public string ProviderKey { get; set; }
        public string ProfileImage { get; set; }
    }
}