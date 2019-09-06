using System;
using Shop.Domain.Commands;
using Shop.Order.Domain.Commands.Dto;

namespace Shop.Order.Domain.Commands
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