using System;
using System.Threading.Tasks;
using AutoMapper;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Dto;
using PokerHand.Common.Dto.Chat;
using PokerHand.Common.Entities.Chat;
using PokerHand.Common.Helpers;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.BusinessLogic.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChatService(
            IUnitOfWork unitOfWork, 
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultModel<ConversationDto>> GetConversationAsync(Guid senderId, Guid recipientId)
        {
            var result = new ResultModel<ConversationDto>();
            
            var conversation = await _unitOfWork.Conversations.FindConversationAsync(senderId, recipientId);

            if (conversation == null)
            {
                result.IsSuccess = false;
                result.Message = "No conversations were found";
                return result;
            }

            result.IsSuccess = true;
            result.Value = _mapper.Map<ConversationDto>(conversation);
            return result;
        }

        public async Task SaveMessageAsync(Guid senderId, Guid recipientId, string messageText)
        {
            // Try to find conversation
            var conversation = await _unitOfWork.Conversations.FindConversationAsync(senderId, recipientId);
            
            // If no conversations were found, create new conversation
            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    FirstPlayerId = senderId,
                    SecondPlayerId = recipientId
                };

                await _unitOfWork.Conversations.AddAsync(conversation);
            }
                
            // Add a message to conversation
            var message = new Message
            {
                Id = Guid.NewGuid(),
                Text = messageText,
                TimeCreated = DateTime.Now,
                PlayerId = senderId,
                ConversationId = conversation.Id
            };
            
            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.CompleteAsync();
        }
    }
}