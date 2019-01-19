using System;
using Shop.Cqrs.Events;

namespace Shop.Domain.Commands
{
    public interface ICorrelatedEvent : IEvent
    {
        Guid CorrelationId { get; }
    }
}