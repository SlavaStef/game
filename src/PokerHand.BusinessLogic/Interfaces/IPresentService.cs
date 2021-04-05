using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokerHand.Common.Helpers.Present;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IPresentService
    {
        event Action<string> OnSendPresent;
            
        void SendPresent(Guid senderId, Guid[] recipientIds, PresentName presentName);
    }
}