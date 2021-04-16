using PokerHand.Common.Helpers.Player;

namespace PokerHand.Common.ViewModels.Auth
{
    public class RegisterAsGuestVM
    {
        public string UserName { get; set; }
        public Gender Gender { get; set; }
        public HandsSpriteType HandsSprite { get; set; }
    }
}