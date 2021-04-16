using PokerHand.Common.Helpers.Authorization;
using PokerHand.Common.Helpers.Player;

namespace PokerHand.Common.ViewModels.Auth
{
    public class RegisterWithExternalProviderVM
    {
        public string UserName { get; set; }
        public Gender Gender { get; set; }
        public HandsSpriteType HandsSprite { get; set; }
        public ExternalProviderName ProviderName { get; set; }
        public string ProviderKey { get; set; }
    }
}