using System;
using System.Linq.Expressions;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public class QueueConfigurator<TQueue> : QueueConfiguratorBase, IQueueConfiguration<TQueue> where TQueue : IQueueMap
    {
        public void Configure<TCommand>(Expression<Func<TQueue,TCommand>> commandSelector, Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
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