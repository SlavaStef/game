using AutoMapper;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;

namespace PokerHand.Server.AutoMapper
{
    public class Profiles : Profile
    {
        public Profiles()
        {
            CreateMap<Table, TableDto>();
        }
    }
}