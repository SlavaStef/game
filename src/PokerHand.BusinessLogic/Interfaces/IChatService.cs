using System;
using System.Threading.Tasks;
using PokerHand.Common.Dto;
using PokerHand.Common.Dto.Chat;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IChatService
    {
        Task<ResultModel<ConversationDto>> GetConversationAsync(Guid senderId, Guid recipientId);
        Task SaveMessageAsync(Guid senderId, Guid recipientId, string messageText);
    }
}