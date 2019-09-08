using AutoMapper;
using Shop.Catalog.DataAccess.Dto;
using Shop.Catalog.ServiceModels;

namespace Shop.Catalog.ServiceEndpoint
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, DataProjections.Models.Product>();

            CreateMap<DataProjections.Models.Product, ProductModel>();
        }
    }
}