using System;
using System.Linq.Expressions;
using Automatonymous;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public interface IServiceConfiguration<TService> where TService : IServiceMap
    {
        void Configure<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configure = null) where TQueue : IQueueMap;

        void ConfigureAsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector, Type sagaInstanceType)
            where TQueue : IQueueMap;
    }
}