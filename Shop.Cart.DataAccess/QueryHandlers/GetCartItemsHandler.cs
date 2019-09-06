using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassInstance.Cqrs.Queries;
using Microsoft.EntityFrameworkCore;
using Shop.Cart.DataProjections.Models;
using Shop.Cart.DataProjections.Queries;

namespace Shop.Cart.DataAccess.QueryHandlers
{
    public class GetCartItemsHandler : IQueryHandler<GetCartItems, IEnumerable<CartItem>>
    {
        private readonly CartDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetCartItemsHandler(CartDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItem>> HandleAsync(GetCartItems query, CancellationToken cancellationToken)
        {
            var items = await _dbContext
                .CartItems
                .Where(i => i.SessionId == query.SessionId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<CartItem>>(items);
        }
    }
}