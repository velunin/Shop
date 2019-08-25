using System;

namespace Shop.Domain.Order.Events
{
    public class OrderCreated : ICorrelatedEvent
    {
        public OrderCreated(Guid orderId)
        {
            CorrelationId = orderId;
            OrderId = orderId;
        }

        public Guid CorrelationId { get; set; }

        public Guid OrderId { get; }
    }
}