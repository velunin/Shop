using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Automatonymous;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public interface IQueueConfiguration<TQueue> : IQueueConfiguration where TQueue : IQueueMap
    {
        ICommandConfiguration SelectCommand<TCommand>(Expression<Func<TQueue, TCommand>> commandSelector);
    }

    public interface IQueueConfiguration
    {
        ICommandConfiguration SelectCommand<TQueue,TCommand>(Expression<Func<TQueue, TCommand>> commandSelector) 
            where TQueue : IQueueMap;

        void ConfigureSaga<TSagaInstance>() where TSagaInstance : SagaStateMachineInstance;

        ICommandConfiguration GetConfigurationForCommand(Type commandType);

        IEnumerable<Type> GetSagaInstanceTypes();

        bool TryGetCommandConfig(Type commandType, out ICommandConfiguration commandConfiguration);

        Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }
    }
}