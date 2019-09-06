using System;
using MassInstance.Cqrs.Events;

namespace Shop.Order.Domain.Events
{
    public class OrderHasBeenPayed : IEvent
    {
        public OrderHasBeenPayed(Guid orderId)
        {   
            OrderId = orderId;
        }

        public Guid CorrelationId => OrderId;

        public Guid OrderId { get; }
    }
}