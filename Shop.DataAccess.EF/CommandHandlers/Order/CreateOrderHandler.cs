using System.Threading;
using System.Threading.Tasks;

using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;

using Shop.Domain.Commands.Order;
using Shop.Domain.Events;

namespace Shop.DataAccess.EF.CommandHandlers.Order
{
    public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
    {
        private readonly IEventDispatcher _eventDispatcher;

        public CreateOrderHandler(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public async Task HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            await _eventDispatcher.DispatchAsync(
                new OrderCreated(command.OrderId),
                cancellationToken)
                .ConfigureAwait(false);
        }
    }
}