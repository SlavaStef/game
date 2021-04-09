using System;
using PokerHand.Common.Helpers.Player;

namespace PokerHand.Common.ViewModels.Profile
{
    public class UpdateProfileVM
    {
        public Guid Id { get; set; }
        public Gender Gender { get; set; }
        public CountryCode Country { get; set; }
        public HandsSpriteType HandsSprite { get; set; }
        public string UserName { get; set; }
    }
}