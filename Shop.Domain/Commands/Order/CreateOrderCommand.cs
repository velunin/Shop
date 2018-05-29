using System;

namespace Shop.Domain.Commands.Order
{
    public class CreateOrderCommand : ICorrelatedCommand
    {
        public CreateOrderCommand(Guid orderId, OrderItem[] orderItems)
        {
            OrderItems = orderItems;
            OrderId = orderId;
        }

        public Guid OrderId { get; }

        public OrderItem[] OrderItems { get; set; }

        public Guid CorrelationId => OrderId;
    }
}