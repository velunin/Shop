using System;
using MassInstance.Cqrs.Commands;

namespace Shop.Domain.Commands
{
    public interface ICorrelatedCommand : ICommand
    {
        Guid CorrelationId { get; }
    }
}