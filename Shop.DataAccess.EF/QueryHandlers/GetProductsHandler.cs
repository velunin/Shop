using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Rds.Cqrs.Queries;
using Shop.DataProjections.Models;
using Shop.DataProjections.Queries;

namespace Shop.DataAccess.EF.QueryHandlers
{
    public class GetProductsHandler : IQueryHandler<GetProducts, IEnumerable<Product>>
    {
        private readonly ShopDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetProductsHandler(ShopDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Product>> HandleAsync(GetProducts query, CancellationToken cancellationToken)
        {
            var products = await _dbContext.Product.ToListAsync(cancellationToken).ConfigureAwait(false);

            return _mapper.Map<IEnumerable<Product>>(products);
        }
    }
}