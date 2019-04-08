using System;
using System.Linq.Expressions;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public interface IServiceConfiguration<TService> : IServiceConfiguration where TService : IServiceMap
    {
        void Configure<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null) where TQueue : IQueueMap;
    }

    public interface IServiceConfiguration
    {
        void Configure<TService,TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null) 
            where TQueue : IQueueMap 
            where TService : IServiceMap;

        bool TryGetQueueConfig(string queueName, out IQueueConfiguration queueConfiguration);

        Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }
    }
}