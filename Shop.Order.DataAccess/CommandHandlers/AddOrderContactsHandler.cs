using System.Threading;
using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shop.Order.Domain.Commands;
using Shop.Order.Domain.Events;

namespace Shop.Order.DataAccess.CommandHandlers
{
    public class AddOrderContactsHandler : ICommandHandler<AddOrderContactsCommand>
    {
        private readonly IBus _bus;
        private readonly OrderDbContext _context;

        public AddOrderContactsHandler(IBus bus, OrderDbContext context)
        {
            _bus = bus;
            _context = context;
        }

        public async Task HandleAsync(AddOrderContactsCommand command, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
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