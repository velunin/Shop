using System;
using NSaga;
using Shop.Domain.Commands;

namespace Shop.Domain.Events
{
    public class OrderCreated : ICorrelatedEvent, ISagaMessage
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