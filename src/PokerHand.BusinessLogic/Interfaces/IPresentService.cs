using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokerHand.Common.Dto;
using PokerHand.Common.Helpers.Present;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IPresentService
    {
        event Action<string> OnSendPresent;
        event Action<string> OnSendPresentError;
        event Action<string> SendTotalMoneyAmount;

        List<PresentInfoDto> GetAllPresentsInfo();
        Task SendPresent(Guid senderId, List<Guid> recipientsIds, PresentName presentName);
    }
}