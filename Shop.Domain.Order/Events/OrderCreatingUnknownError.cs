using System;

namespace Shop.Domain.Order.Events
{
    public class OrderCreatingUnknownError : ICorrelatedEvent
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