using System;
using System.Linq.Expressions;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public interface IServiceConfiguration<TService> where TService : IServiceMap
    {
        void ForQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configure = null) where TQueue : IQueueMap;

        void SetAsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector) where TQueue : IQueueMap;
    }
}