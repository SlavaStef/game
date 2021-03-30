using System;

namespace PokerHand.Common.Helpers.Player
{
    public class PlayerProfileUpdateForm
    {
        public Guid Id { get; set; }
        public Gender Gender { get; set; }
        public CountryName Country { get; set; }
        public HandsSpriteType HandsSprite { get; set; }
        public string UserName { get; set; }
    }
}