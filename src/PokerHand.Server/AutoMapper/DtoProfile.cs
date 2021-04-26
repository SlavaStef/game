using System.Linq;
using AutoMapper;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Table;

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
        }
    }
}