using System;
using System.Linq.Expressions;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public interface IQueueConfiguration<TQueue> : IQueueConfiguration where TQueue : IQueueMap
    {
        void Configure<TCommand>(Expression<Func<TQueue, TCommand>> commandSelector, Action<ICommandConfiguration> configureCommand = null);
    }

    public interface IQueueConfiguration
    {
        void Configure<TQueue,TCommand>(Expression<Func<TQueue, TCommand>> commandSelector,
            Action<ICommandConfiguration> configureCommand = null) 
            where TQueue : IQueueMap;

        ICommandConfiguration GetConfigurationForCommand(Type commandType);

        Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        bool IsSagaQueue { get; }

        Type SagaInstanceType { get; }
    }
}