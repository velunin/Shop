using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Rds.Cqrs.Commands;

using Shop.Domain.Commands.Cart;

namespace Shop.DataAccess.EF.CommandHandlers.Cart
{
    public class DeleteProductFromCartHandler : ICommandHandler<DeleteProductFromCart>
    {
        private readonly ShopDbContext _dbContext;

        public DeleteProductFromCartHandler(ShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleAsync(DeleteProductFromCart command, CancellationToken cancellationToken)
        {
            var cartItem = await _dbContext
                .CartItem
                .SingleAsync(c => c.ProductId == command.ProductId && c.SessionId == command.SessionId,
                    cancellationToken);

            _dbContext.Remove(cartItem);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}