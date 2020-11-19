using AutoMapper;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;

namespace PokerHand.Server.AutoMapper
{
    public class Profiles : Profile
    {
        public Profiles()
        {
            CreateMap<Player, PlayerDto>();
            CreateMap<Table, TableDto>()
                .ForMember(dest => dest.Players, opt => opt.MapFrom(
                    src => src.Players));
        }
    }
}