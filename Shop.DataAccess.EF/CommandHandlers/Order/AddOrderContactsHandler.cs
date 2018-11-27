using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Rds.Cqrs.Commands;
using Shop.Domain.Commands.Order;
using Shop.Domain.Events;

namespace Shop.DataAccess.EF.CommandHandlers.Order
{
    public class AddOrderContactsHandler : ICommandHandler<AddOrderContactsCommand>
    {
        private readonly IBus _bus;
        private readonly ShopDbContext _context;

        public AddOrderContactsHandler(IBus bus, ShopDbContext context)
        {
            _bus = bus;
            _context = context;
        }

        public async Task HandleAsync(AddOrderContactsCommand command, CancellationToken cancellationToken)
        {
            var order = await _context.Order
                .SingleAsync(o => o.OrderId == command.OrderId, cancellationToken)
                .ConfigureAwait(false);

            order.Name = command.Name;
            order.Email = command.Email;
            order.Phone = command.Phone;

            _context.Attach(order);

            await _context.SaveChangesAsync(cancellationToken);

            await _bus.Publish(
                new OrderContactsAdded(
                    command.OrderId, 
                    command.Name,
                    command.Email, 
                    command.Phone),
                cancellationToken);
        }
    }
}