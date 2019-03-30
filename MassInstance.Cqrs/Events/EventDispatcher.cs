using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MassInstance.Cqrs.Events
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IHandlerResolver _handlerResolver;

        private readonly ConcurrentDictionary<Type, (Type, Func<object, IEvent, CancellationToken, Task>)>
            _eventHandlersInvokers =
                new ConcurrentDictionary<Type, (Type, Func<object, IEvent, CancellationToken, Task>)>();

        public EventDispatcher(IHandlerResolver handlerResolver)
        {
            _handlerResolver = handlerResolver ?? throw new ArgumentNullException(nameof(handlerResolver));
        }

        public async Task DispatchAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            var eventType = @event.GetType();

            var (eventHandlerType, handleInvoker) = _eventHandlersInvokers.GetOrAdd(eventType, CreateHandlerInvoker);

            var eventHandlers = _handlerResolver.ResolveHandlers(eventHandlerType);

            var handlersTasks = eventHandlers?
                                    .Select(eh => handleInvoker(eh, @event, cancellationToken))
                                ?? Enumerable.Empty<Task>();

            var tcs = new TaskCompletionSource<object>();
            cancellationToken.Register(
                () => tcs.TrySetCanceled(), 
                false);

            await Task.WhenAny(
                    Task.WhenAll(handlersTasks),
                    tcs.Task)
                .ConfigureAwait(false);
        }

        private static (Type, Func<object, IEvent, CancellationToken, Task>) CreateHandlerInvoker(Type eventType)
        {
            var eventHandlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

            var handleAsyncMethod =
                eventHandlerType.GetMethod(
                    "HandleAsync",
                    BindingFlags.Instance | BindingFlags.Public);

            var eventHandlerParameter = Expression.Parameter(typeof(object));
            var eventParameter = Expression.Parameter(typeof(IEvent));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

            var eventHandlerVariable = Expression.Variable(eventHandlerType, "eventHandler");
            var eventVariable = Expression.Variable(eventType, "@event");

            var block = Expression.Block(
                new[] {eventHandlerVariable, eventVariable},
                Expression.Assign(
                    eventHandlerVariable, 
                    Expression.Convert(eventHandlerParameter, eventHandlerType)), 
                Expression.Assign(
                    eventVariable, 
                    Expression.Convert(eventParameter, eventType)),
                Expression.Call(
                    eventHandlerVariable, 
                    handleAsyncMethod, 
                    eventVariable, 
                    cancellationTokenParameter));

            var lambda = Expression.Lambda<Func<object, IEvent, CancellationToken, Task>>(
                block, 
                eventHandlerParameter,
                eventParameter, 
                cancellationTokenParameter);

            return (eventHandlerType, lambda.Compile());
        }
    }
}