using System;

namespace PokerHand.Common.ViewModels.Auth
{
    public class AuthenticateVM
    {
        public Guid UserId { get; set; }
        public string ConnectionId { get; set; }
    }
}