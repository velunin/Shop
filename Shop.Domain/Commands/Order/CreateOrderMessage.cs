using System;
using NSaga;
using Shop.Domain.Sagas;

namespace Shop.Domain.Commands.Order
{
    public class CreateOrderMessage : CorrelationCommandMessage<CreateOrderCommand>, IInitiatingSagaMessage
    {
        public CreateOrderMessage(Guid orderId, OrderItem[] orderItems) : base(
            new CreateOrderCommand(
                orderId, 
                orderItems))
        {
        }
    }
}