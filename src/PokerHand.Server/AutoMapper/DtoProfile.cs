using AutoMapper;
using PokerHand.Common.Dto;
using PokerHand.Common.Dto.Chat;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Table;
using PokerHand.Common.Entities.Chat;

namespace PokerHand.Server.AutoMapper
{
    public class DtoProfile : Profile
    {
        public DtoProfile()
        {
            CreateMap<Player, PlayerDto>();
            CreateMap<Player, PlayerProfileDto>().ForMember(dest => dest.ProviderName,
                opt => opt.MapFrom(src => src.PlayerLogin.ProviderName));
            CreateMap<Table, TableDto>();
            CreateMap<SidePot, SidePotDto>();
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderId, 
                    opt => opt.MapFrom(src => src.PlayerId))
                .ForMember(dest => dest.Text, 
                    opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.TimeCreated, 
                    opt => opt.MapFrom(src => src.TimeCreated));
            CreateMap<Conversation, ConversationDto>()
                .ForMember(dest => dest.Messages, 
                    opt => opt.MapFrom(src => src.Messages));;
        }
    }
}