using System;
using System.Linq.Expressions;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public interface IServiceConfiguration<TService> : IServiceConfiguration where TService : IServiceMap
    {
        IQueueConfiguration<TQueue> SelectQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector) where TQueue : IQueueMap;
    }

    public interface IServiceConfiguration
    {
        IQueueConfiguration<TQueue> SelectQueue<TService,TQueue>(
            Expression<Func<TService, TQueue>> queueSelector) 
            where TQueue : IQueueMap 
            where TService : IServiceMap;

        bool TryGetQueueConfig(string queueName, out IQueueConfiguration queueConfiguration);

        Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }
    }
}