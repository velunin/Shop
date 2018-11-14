using AutoMapper;
using Shop.DataProjections.Models;
using Shop.Web.Models;

namespace Shop.Web.MappingProfiles
{
    public class ViewModelsMappingProfile : Profile
    {
        public ViewModelsMappingProfile()
        {
            CreateMap<Product, ProductViewModel>();
            CreateMap<CartItem, CartItemModel>();
        }
    }
}