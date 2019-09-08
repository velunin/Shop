using System;
using MassInstance.Cqrs.Commands;
using Shop.Order.Domain.Commands.Dto;

namespace Shop.Order.Domain.Commands
{
    public class CreateOrderCommand : ICommand
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