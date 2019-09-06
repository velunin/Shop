using AutoMapper;
using Shop.Cart.DataAccess.Dto;

namespace Shop.Cart.DataAccess
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CartItem, DataProjections.Models.CartItem>();
        }
    }
}