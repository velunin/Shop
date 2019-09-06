using System.Threading;
using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shop.Order.DataProjections;
using Shop.Order.Domain.Commands;
using Shop.Order.Domain.Commands.Results;
using Shop.Order.Domain.Events;

namespace Shop.Order.DataAccess.CommandHandlers
{
    public class PayOrderHandler : ICommandHandler<PayOrderCommand, PayOrderResult>
    {
        private readonly IBus _bus;
        private readonly OrderDbContext _dbContext;

        public PayOrderHandler(IBus bus, OrderDbContext dbContext)
        {
            _bus = bus;
            _dbContext = dbContext;
        }

        public async Task<PayOrderResult> HandleAsync(PayOrderCommand command, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .SingleAsync(o => o.OrderId == command.OrderId, cancellationToken)
                .ConfigureAwait(false);

            order.Status = OrderStatus.Payed;

            _dbContext.Attach(order);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _bus.Publish(
                new OrderHasBeenPayed(command.OrderId), cancellationToken).ConfigureAwait(false);

            return new PayOrderResult { Message = "Some message in pay order result" };
        }
    }
}