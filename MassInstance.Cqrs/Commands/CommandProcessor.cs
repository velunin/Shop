using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MassInstance.Cqrs.Commands
{
    public class CommandProcessor : ICommandProcessor
    {
        private readonly IHandlerResolver _handlerResolver;
        private readonly ConcurrentDictionary<Type, (Type, Func<object, ICommand, CancellationToken, Task>)>
            _handlerInvokersCache =
                new ConcurrentDictionary<Type, (Type, Func<object, ICommand, CancellationToken, Task>)>();

        public CommandProcessor(IHandlerResolver handlerResolver)
        {
            _handlerResolver = handlerResolver ?? throw new ArgumentNullException(nameof(handlerResolver));
        }

        public async Task ProcessAsync(
            ICommand command, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();

            if (!_handlerInvokersCache.TryGetValue(commandType, out var invokerTuple))
            {
                invokerTuple = CreateHandlerInvoker(
                    commandType,
                    () => typeof(ICommandHandler<>).MakeGenericType(commandType));
                _handlerInvokersCache.TryAdd(commandType, invokerTuple);
            }

            var (commandHandlerType, handlerInvoker) = invokerTuple;

            var commandHandlerInstance = _handlerResolver.ResolveHandler(commandHandlerType);

            await handlerInvoker(
                    commandHandlerInstance,
                    command,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TResult> ProcessAsync<TResult>(
            IResultingCommand<TResult> command,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();

            if (!_handlerInvokersCache.TryGetValue(commandType, out var invokerTuple))
            {
                invokerTuple = CreateHandlerInvoker(
                    commandType,
                    () => typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult)));
                _handlerInvokersCache.TryAdd(commandType, invokerTuple);
            }

            var (commandHandlerType, handlerInvoker) = invokerTuple;

            var commandHandlerInstance = _handlerResolver.ResolveHandler(commandHandlerType);

            return await (Task<TResult>)handlerInvoker(
                commandHandlerInstance,
                command,
                cancellationToken);
        }

        private static (Type, Func<object, ICommand, CancellationToken, Task>) CreateHandlerInvoker(
            Type commandType,
            Func<Type> handlerTypeFactory)
        {
            var handlerType = handlerTypeFactory();

            var handleMethod = handlerType.GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.Public);

            var lambdaHandlerParameter = Expression.Parameter(typeof(object));
            var lambdaCommandParameter = Expression.Parameter(typeof(ICommand));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

            var handlerInstance = Expression.Variable(handlerType, "commandHandler");
            var concreteCommand = Expression.Variable(commandType, "command");

            var block = Expression.Block(
                new[] { handlerInstance, concreteCommand },
                Expression.Assign(
                    handlerInstance, 
                    Expression.Convert(lambdaHandlerParameter, handlerType)),
                Expression.Assign(
                    concreteCommand,
                    Expression.Convert(lambdaCommandParameter, commandType)),
                Expression.Call(
                    handlerInstance,
                    handleMethod, 
                    concreteCommand, 
                    cancellationTokenParameter));

            var lambda =
                Expression.Lambda<Func<object, ICommand, CancellationToken, Task>>(
                    block, 
                    lambdaHandlerParameter,
                    lambdaCommandParameter,
                    cancellationTokenParameter);

            return (handlerType, lambda.Compile());
        }
    }
}