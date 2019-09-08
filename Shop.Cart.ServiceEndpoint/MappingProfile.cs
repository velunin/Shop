using AutoMapper;
using Shop.Cart.DataAccess.Dto;
using Shop.Cart.ServiceModels;
using Shop.Order.Domain.Commands.Dto;

namespace Shop.Cart.ServiceEndpoint
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CartItem, DataProjections.Models.CartItem>();

            CreateMap<DataProjections.Models.CartItem, CartItemModel>();
            CreateMap<DataProjections.Models.CartItem, OrderItem>();
        }
    }
}