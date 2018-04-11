using System;
using Rds.Cqrs.Commands;

namespace Shop.Domain.Commands
{
    public interface ICorrelatedCommand : ICommand
    {
        Guid CorrelationId { get; }
    }
}