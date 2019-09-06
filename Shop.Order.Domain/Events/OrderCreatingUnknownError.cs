using System;
using MassInstance.Cqrs.Events;

namespace Shop.Order.Domain.Events
{
    public class OrderCreatingUnknownError : IEvent
    {
        public OrderCreatingUnknownError(Guid orderId)
        {
            CorrelationId = orderId;
            OrderId = orderId;
        }

        public Guid CorrelationId { get; set; }

        public Guid OrderId { get; }
    }
}