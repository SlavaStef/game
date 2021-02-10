using AutoMapper;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;

namespace PokerHand.Server.AutoMapper
{
    public class DtoProfile : Profile
    {
        public DtoProfile()
        {
            CreateMap<Player, PlayerDto>();
            CreateMap<Player, PlayerProfileDto>();
            CreateMap<Table, TableDto>();
            CreateMap<SidePot, SidePotDto>();
        }
    }
}