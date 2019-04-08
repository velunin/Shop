using System.Threading;
using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using MassTransit;

using Microsoft.EntityFrameworkCore;
using Shop.DataProjections.Models;
using Shop.Domain.Commands.Order;
using Shop.Domain.Commands.Order.Results;
using Shop.Domain.Events;

namespace Shop.DataAccess.EF.CommandHandlers.Order
{
    public class PayOrderHandler : ICommandHandler<PayOrderCommand, PayOrderResult>
    {
        private readonly IBus _bus;
        private readonly ShopDbContext _dbContext;

        public PayOrderHandler(IBus bus, ShopDbContext dbContext)
        {
            _bus = bus;
            _dbContext = dbContext;
        }

        public async Task<PayOrderResult> HandleAsync(PayOrderCommand command, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Order
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