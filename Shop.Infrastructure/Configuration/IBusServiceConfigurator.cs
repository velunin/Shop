using System;
using MassTransit;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;

namespace Shop.Infrastructure.Configuration
{
    public interface IBusServiceConfigurator
    {
        IBusServiceConfigurator AddCommandConsumer<TCommand>(Action<CommandExceptionHandlingOptions> exceptionHandlingConfigure = null) where TCommand : ICommand;

        IBusServiceConfigurator AddEventConsumer<TEvent>() where TEvent : class, IEvent;

        void Configure(IReceiveEndpointConfigurator endpointConfigurator, IServiceProvider provider);
    }
}