using System;
using System.Linq.Expressions;
using Shop.Services.Common;

namespace Shop.Infrastructure.Configuration
{
    public interface IQueueConfiguration<TQueue> where TQueue : IQueueMap
    {
        void ForCommand<TCommand>(Expression<Func<TQueue, TCommand>> commandSelector, Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }
}