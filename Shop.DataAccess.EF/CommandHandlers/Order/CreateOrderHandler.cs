using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Rds.Cqrs.Commands;
using Shop.Domain.Commands.Order;
using Shop.Domain.Events;

namespace Shop.DataAccess.EF.CommandHandlers.Order
{
    public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
    {
        private readonly IBus _bus;

        public CreateOrderHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            if (command.OrderItems.Any(x => x.ProductId == Guid.Parse("C09DDC26-FCF8-4EEB-B178-E93C01B81D92")))
            {
                throw new Exception("Some error");
            }

            if (command.OrderItems.Any(x => x.ProductId == Guid.Parse("EA8D2304-2103-416C-99A2-3DF694CF2FEE")))
            {
                throw new InvalidOperationException("Already sold");
            }

            await _bus.Publish(
                new OrderCreated(command.OrderId),
                cancellationToken)
                .ConfigureAwait(false);
        }
    }
}