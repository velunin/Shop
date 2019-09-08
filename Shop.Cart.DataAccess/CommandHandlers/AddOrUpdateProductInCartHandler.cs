using System.Threading;
using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.Cart.DataAccess.Dto;
using Shop.Cart.Domain.Commands;

namespace Shop.Cart.DataAccess.CommandHandlers
{
    public class AddOrUpdateProductInCartHandler : ICommandHandler<AddOrUpdateProductInCart>
    {
        private readonly CartDbContext _dbContext;

        public AddOrUpdateProductInCartHandler(CartDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleAsync(AddOrUpdateProductInCart command, CancellationToken cancellationToken)
        {
            var product = await _dbContext
                .Products
                .SingleOrDefaultAsync(p => p.ProductId == command.ProductId, 
                    cancellationToken);

            var cartItem = await _dbContext
                .CartItems
                .SingleOrDefaultAsync(c => c.ProductId == command.ProductId && c.SessionId == command.SessionId,
                    cancellationToken);

            if (cartItem == null)
            {
                _dbContext.Add(new CartItem
                {
                    SessionId = command.SessionId,
                    ProductId = command.ProductId,
                    Name = product.Name,
                    Price = product.Price,
                    Count = command.Count
                });
            }
            else
            {
                cartItem.Count = command.Count;
                cartItem.Name = product.Name;
                cartItem.Price = product.Price;

                _dbContext.Attach(cartItem);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}