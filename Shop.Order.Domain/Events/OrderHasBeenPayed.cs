using System;
using Shop.Domain.Commands;

namespace Shop.Order.Domain.Events
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