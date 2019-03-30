using System.Threading;
using System.Threading.Tasks;

namespace MassInstance.Cqrs.Events
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}