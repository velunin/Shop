using System;
using System.Collections.Generic;
using MassInstance.Cqrs.Commands;
using MassInstance.Cqrs.Events;
using MassTransit.Saga;

namespace MassInstance.Configuration.Old
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