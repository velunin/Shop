using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using MassTransit;
using Shop.Order.DataAccess.Dto;
using Shop.Order.DataProjections;
using Shop.Order.Domain.Commands;
using Shop.Order.Domain.Events;

namespace Shop.Order.DataAccess.CommandHandlers
{
    public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
    {
        private readonly IBus _bus;
        private readonly OrderDbContext _context;

        public CreateOrderHandler(IBus bus, OrderDbContext context)
        {
            _bus = bus;
            _context = context;
        }

        public async Task HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            await Validate(command, cancellationToken).ConfigureAwait(false);

            using (var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
            {
                await _context.Orders.AddAsync(new Dto.Order
                        {
                            OrderId = command.OrderId,
                            CreateDate = DateTime.Now,
                            Status = OrderStatus.New
                        },
                        cancellationToken)
                    .ConfigureAwait(false);

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in command.OrderItems)
                {
                    await _context.OrderItem.AddAsync(new OrderItem
                            {
                                OrderItemId = Guid.NewGuid(),
                                Name = item.Name,
                                Count = item.Count,
                                OrderId = command.OrderId,
                                Price = item.Price
                            },
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                transaction.Commit();

                await _bus.Publish(
                        new OrderCreated(command.OrderId),
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private async Task Validate(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            if (command.OrderItems.Any(x => x.ProductId == Guid.Parse("C09DDC26-FCF8-4EEB-B178-E93C01B81D92")))
            {
                await _bus.Publish(
                        new OrderCreatingUnknownError(command.OrderId),
                        cancellationToken)
                    .ConfigureAwait(false);

                throw new Exception("Some error");

            }

            if (command.OrderItems.Any(x => x.ProductId == Guid.Parse("EA8D2304-2103-416C-99A2-3DF694CF2FEE")))
            {
                await _bus.Publish(
                        new OrderCreatingAlreadySoldError(command.OrderId),
                        cancellationToken)
                    .ConfigureAwait(false);

                throw new InvalidOperationException("Already sold");
            }
        }
    }
}