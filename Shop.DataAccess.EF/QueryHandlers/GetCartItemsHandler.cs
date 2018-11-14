using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.EntityFrameworkCore;

using Rds.Cqrs.Queries;
using Shop.DataProjections.Models;
using Shop.DataProjections.Queries;

namespace Shop.DataAccess.EF.QueryHandlers
{
    public class GetCartItemsHandler : IQueryHandler<GetCartItems, IEnumerable<CartItem>>
    {
        private readonly ShopDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetCartItemsHandler(ShopDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItem>> HandleAsync(GetCartItems query, CancellationToken cancellationToken)
        {
            var items = await _dbContext
                .CartItem
                .Where(i => i.SessionId == query.SessionId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<CartItem>>(items);
        }
    }
}