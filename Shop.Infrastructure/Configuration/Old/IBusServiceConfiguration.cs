using System;
using System.Collections.Generic;
using MassTransit.Saga;
using Shop.Cqrs.Commands;
using Shop.Cqrs.Events;

namespace Shop.Infrastructure.Configuration
{
    public interface IBusServiceConfiguration
    {
        IBusServiceConfiguration AddCommandConsumer<TCommand>(Action<CommandExceptionHandlingOptions> exceptionHandlingConfigure = null) where TCommand : ICommand;

        IBusServiceConfiguration AddEventConsumer<TEvent>() where TEvent : class, IEvent;

        IBusServiceConfiguration AddSaga<TSaga>() where TSaga : class, ISaga;
       
        IEnumerable<BusServiceConfiguration.CommandConfigItem> GetCommandConfigs();

        IEnumerable<Type> GetEventsTypes();

        IEnumerable<Type> GetSagasTypes();
    }
}