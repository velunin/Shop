using System;
using System.Linq.Expressions;
using Shop.Services.Common;

namespace Shop.Infrastructure.Configuration
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