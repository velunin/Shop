using AutoMapper;
using Shop.DataAccess.Dto;

namespace Shop.DataAccess.MappingProfiles
{
    public class DtoProjectionsMappingProfile : Profile
    {
        public DtoProjectionsMappingProfile()
        {
            //Dto to Projections
            CreateMap<Product, DataProjections.Models.Product>();
            CreateMap<CartItem, DataProjections.Models.CartItem>();

            //Projections to Dto
            CreateMap<DataProjections.Models.CartItem, OrderItem>();
        }
    }
}