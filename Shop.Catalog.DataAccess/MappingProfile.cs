using AutoMapper;
using Shop.Catalog.DataAccess.Dto;

namespace Shop.Catalog.DataAccess
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, DataProjections.Models.Product>();
        }
    }
}