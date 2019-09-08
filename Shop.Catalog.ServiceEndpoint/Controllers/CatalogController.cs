using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MassInstance.Cqrs.Queries;
using Microsoft.AspNetCore.Mvc;
using Shop.Catalog.DataProjections.Queries;
using Shop.Catalog.ServiceModels;

namespace Shop.Catalog.ServiceEndpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly IQueryService _queryService;
        private readonly IMapper _mapper;

        public CatalogController(IQueryService queryService, IMapper mapper)
        {
            _queryService = queryService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductModel>> Get()
        {
            var products = await _queryService.QueryAsync(new GetAllProducts());

            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }
    }
}