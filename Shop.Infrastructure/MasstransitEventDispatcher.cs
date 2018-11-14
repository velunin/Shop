using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Rds.Cqrs.Events;

namespace Shop.Infrastructure
{
    public class MasstransitEventDispatcher : IEventDispatcher
    {
        private readonly IBus _serviceBus;

        public MasstransitEventDispatcher(IBus serviceBus)
        {
            _serviceBus = serviceBus;
        }

        public async Task DispatchAsync(IEvent eventMessage, CancellationToken cancellationToken = new CancellationToken())
        {
            await _serviceBus.Publish(eventMessage, cancellationToken).ConfigureAwait(false);
        }

        public void Dispatch(IEvent eventMessage)
        {
            DispatchAsync(eventMessage)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}