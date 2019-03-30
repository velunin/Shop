using System.Threading;
using System.Threading.Tasks;

namespace MassInstance.Cqrs.Events
{
    public interface IEventDispatcher
    {
        Task DispatchAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}