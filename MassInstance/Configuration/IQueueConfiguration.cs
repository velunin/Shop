using System;
using System.Linq.Expressions;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public interface IQueueConfiguration<TQueue> where TQueue : IQueueMap
    {
        void ForCommand<TCommand>(Expression<Func<TQueue, TCommand>> commandSelector, Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }
}