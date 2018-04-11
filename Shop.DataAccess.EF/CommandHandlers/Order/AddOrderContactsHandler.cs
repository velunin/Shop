using System.Threading;
using System.Threading.Tasks;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;
using Shop.Domain.Commands.Order;
using Shop.Domain.Events;

namespace Shop.DataAccess.EF.CommandHandlers.Order
{
    public class AddOrderContactsHandler : ICommandHandler<AddOrderContactsCommand>
    {
        private readonly IEventDispatcher _eventDispatcher;

        public AddOrderContactsHandler(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public async Task HandleAsync(AddOrderContactsCommand command, CancellationToken cancellationToken)
        {

            await _eventDispatcher.DispatchAsync(
                new OrderContactsAdded(
                    command.OrderId, 
                    command.Name,
                    command.Email, 
                    command.Phone),
                cancellationToken);
        }
    }
}