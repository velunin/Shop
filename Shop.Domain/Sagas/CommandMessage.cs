using System;
using NSaga;
using Rds.Cqrs.Commands;
using Shop.Domain.Commands;

namespace Shop.Domain.Sagas
{
    public abstract class CommandMessage<TCommand> : ISagaMessage where TCommand : ICommand
    {
        protected CommandMessage(TCommand command, Guid correlationId)
        {
            Command = command;
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; set; }

        public TCommand Command { get; }
    }

    public abstract class CorrelationCommandMessage<TCommand> : CommandMessage<TCommand> where TCommand : ICorrelatedCommand
    {
        protected CorrelationCommandMessage(TCommand command) : base(command, command.CorrelationId)
        {
        }

    }
}