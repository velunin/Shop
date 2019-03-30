using System.Threading;
using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.DataAccess.Dto;
using Shop.Domain.Commands.Cart;

namespace Shop.DataAccess.EF.CommandHandlers.Cart
{
    public class AddOrUpdateProductInCartHandler : ICommandHandler<AddOrUpdateProductInCart>
    {
        private readonly ShopDbContext _dbContext;

        public AddOrUpdateProductInCartHandler(ShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleAsync(AddOrUpdateProductInCart command, CancellationToken cancellationToken)
        {
            var product = await _dbContext
                .Product
                .SingleOrDefaultAsync(p => p.ProductId == command.ProductId, 
                    cancellationToken);

            var cartItem = await _dbContext
                .CartItem
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