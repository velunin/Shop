using System;

namespace Shop.Domain.Order.Events
{
    public class OrderHasBeenPayed : ICorrelatedEvent
    {
        public OrderHasBeenPayed(Guid orderId)
        {   
            OrderId = orderId;
        }

        public Guid CorrelationId => OrderId;

        public Guid OrderId { get; }
    }
}