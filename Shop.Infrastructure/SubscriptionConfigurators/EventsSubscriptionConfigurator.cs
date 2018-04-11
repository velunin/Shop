using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rds.Cqrs.Events;
using RDS.CaraBus;

namespace Shop.Infrastructure.SubscriptionConfigurators
{
    internal class EventsSubscriptionConfigurator : ISubscriptionConfigurator
    {
        private readonly ICaraBus _caraBus;
        private readonly Func<IEventDispatcher> _underlyingDispatcherAccessor;

        public EventsSubscriptionConfigurator(
            ICaraBus caraBus, 
            Func<IEventDispatcher> underlyingDispatcherAccessor)
        {
            _caraBus = caraBus;
            _underlyingDispatcherAccessor = underlyingDispatcherAccessor;
        }

        public async Task ConfigureAsync(IEnumerable<Type> sourceTypes, CancellationToken cancellationToken)
        {
            foreach (var allowedEventType in sourceTypes)
            {
                await _caraBus.SubscribeAsync(
                    allowedEventType,
                    async (message, token) => await _underlyingDispatcherAccessor().DispatchAsync(message as IEvent, token),
                    SubscribeOptions.NonExclusive(), 
                    cancellationToken);
            }
        }
    }
}