using System;
using Shop.Domain.Order.Commands.Dto;

namespace Shop.Domain.Order.Commands
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