using AutoMapper;
using Shop.Order.Domain.Commands.Dto;

namespace Shop.Cart.ServiceEndpoint
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DataProjections.Models.CartItem, OrderItem>();
        }
    }
}