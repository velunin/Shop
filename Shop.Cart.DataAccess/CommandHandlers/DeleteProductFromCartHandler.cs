using System.Threading;
using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.Cart.Domain.Commands;

namespace Shop.Cart.DataAccess.CommandHandlers
{
    public class DeleteProductFromCartHandler : ICommandHandler<DeleteProductFromCart>
    {
        private readonly CartDbContext _dbContext;

        public DeleteProductFromCartHandler(CartDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleAsync(DeleteProductFromCart command, CancellationToken cancellationToken)
        {
            var cartItem = await _dbContext
                .CartItems
                .SingleAsync(c => c.ProductId == command.ProductId && c.SessionId == command.SessionId,
                    cancellationToken);

            _dbContext.Remove(cartItem);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}