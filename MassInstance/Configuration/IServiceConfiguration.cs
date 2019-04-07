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

        void ConfigureAsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector, Type sagaInstanceType)
            where TQueue : IQueueMap;
    }

    public interface IServiceConfiguration
    {
        void Configure<TService,TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null) 
            where TQueue : IQueueMap 
            where TService : IServiceMap;

        void ConfigureAsSaga<TService, TQueue>(Expression<Func<TService, TQueue>> queueSelector, Type sagaInstanceType)
            where TQueue : IQueueMap 
            where TService : IServiceMap;

        IQueueConfiguration GetConfigurationForQueue(Type queueType);

        bool TryGetConfigurationForQueue(Type queueType, out IQueueConfiguration queueConfiguration);

        Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }
    }
}