using System;
using System.Linq.Expressions;
using Shop.Services.Common;

namespace Shop.Infrastructure.Configuration
{
    public class QueueConfigurator<TQueue> : QueueConfiguratorBase, IQueueConfiguration<TQueue> where TQueue : IQueueMap
    {
        public void ForCommand<TCommand>(Expression<Func<TQueue,TCommand>> commandSelector, Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            CommandExceptionHanlingConfigActions.Add(commandSelector.ReturnType, configureExceptionHandling);
        }

        public QueueConfigurator(
            ICommandConsumerFactory commandConsumerFactory, 
            IExceptionResponseResolver exceptionResponseResolver) 
            : base(
                commandConsumerFactory, 
                exceptionResponseResolver,
                typeof(TQueue))
        {
        }
    }
}