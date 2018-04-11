using System.Threading;
using System.Threading.Tasks;
using Rds.Cqrs.Events;
using RDS.CaraBus;

namespace Rds.Cqrs.Decorators.Events
{
    public class CaraBusEventDispatcher : IEventDispatcher
    {
        private readonly ICaraBus _caraBus;

        public CaraBusEventDispatcher(ICaraBus caraBus)
        {
            _caraBus = caraBus;
        }

        public Task DispatchAsync(IEvent eventMessage, CancellationToken cancellationToken = new CancellationToken())
        {
            return _caraBus.PublishAsync(eventMessage, cancellationToken: cancellationToken);
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