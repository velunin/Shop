using System.Threading.Tasks;
using MassTransit;
using Shop.Cqrs.Events;

namespace Shop.Infrastructure.Messaging
{
    public class EventConsumer<TEvent> : IConsumer<TEvent> where TEvent : class, IEvent
    {
        private readonly IEventDispatcher _eventDispatcher;

        public EventConsumer(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public async Task Consume(ConsumeContext<TEvent> context)
        {
            await _eventDispatcher
                .DispatchAsync(context.Message, context.CancellationToken)
                .ConfigureAwait(false);
        }
    }
}