using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassInstance.Cqrs.Queries;
using Microsoft.EntityFrameworkCore;
using Shop.Catalog.DataProjections.Models;
using Shop.Catalog.DataProjections.Queries;

namespace Shop.Catalog.DataAccess.QueryHandlers
{
    public class GetAllProductsHandler : IQueryHandler<GetAllProducts, IEnumerable<Product>>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetAllProductsHandler(CatalogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Product>> HandleAsync(GetAllProducts query, CancellationToken cancellationToken)
        {
            var products = await _dbContext.Products.ToListAsync(cancellationToken).ConfigureAwait(false);

            return _mapper.Map<IEnumerable<Product>>(products);
        }
    }
}