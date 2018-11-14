using System;
using System.Threading.Tasks;
using MassTransit;
using Rds.Cqrs.Events;

namespace Shop.Infrastructure.Messaging
{
    public class EventConsumer<TEvent> : IConsumer<TEvent> where TEvent : class, IEvent
    {
        private readonly Func<IEventDispatcher> _underlyingDispatcherFactory;

        public EventConsumer(Func<IEventDispatcher> underlyingDispatcherFactory)
        {
            _underlyingDispatcherFactory = underlyingDispatcherFactory;
        }

        public async Task Consume(ConsumeContext<TEvent> context)
        {
            await _underlyingDispatcherFactory()
                .DispatchAsync(context.Message, context.CancellationToken)
                .ConfigureAwait(false);
        }
    }
}