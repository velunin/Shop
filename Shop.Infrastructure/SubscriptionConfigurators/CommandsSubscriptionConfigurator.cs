using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;
using RDS.CaraBus;

namespace Shop.Infrastructure.SubscriptionConfigurators
{
    internal class CommandsSubscriptionConfigurator : ISubscriptionConfigurator
    {
        private readonly ICaraBus _caraBus;
        private readonly ICommandProcessor _commandProcessor;
        private readonly Func<IEventDispatcher> _underlyingDispatcherAccessor;

        public CommandsSubscriptionConfigurator(
            ICaraBus caraBus, 
            ICommandProcessor commandProcessor, 
            Func<IEventDispatcher> underlyingDispatcherAccessor)
        {
            _caraBus = caraBus;
            _commandProcessor = commandProcessor;
            _underlyingDispatcherAccessor = underlyingDispatcherAccessor;
        }

        public async Task ConfigureAsync(IEnumerable<Type> sourceTypes, CancellationToken cancellationToken)
        {
            foreach (var commandType in sourceTypes)
            {
                var faultMessageCreator = ReflectionHelper.GetFaultMessageCreator(commandType);

                await _caraBus.SubscribeAsync(commandType,
                    async (message, token) => await OnRecievedCommand(message, faultMessageCreator, token),
                    SubscribeOptions.Exclusive(commandType.FullName), cancellationToken);

                var commandExecutedEventType = IsResultingCommand(commandType)
                    ? CreateResultingCommandExecutedType(commandType)
                    : CreateCommandExecutedType(commandType);

                await _caraBus.SubscribeAsync(commandExecutedEventType,
                    async (message,token) => await OnEventRecieved(message, cancellationToken),
                    SubscribeOptions.NonExclusive(), cancellationToken);
            }
        }

        private static Type CreateCommandExecutedType(Type commandType)
        {
            return typeof(CommandExecuted<>).MakeGenericType(commandType);
        }

        private static Type CreateResultingCommandExecutedType(Type commandType)
        {
            return typeof(ResultingCommandExecuted<,>)
                .MakeGenericType(
                    commandType,
                    commandType.GenericTypeArguments.First());
        }

        private static bool IsResultingCommand(Type commandType)
        {
            return commandType.GetInterfaces().Any(x => x == typeof(IResultingCommand<>));
        }

        private async Task OnEventRecieved(object message, CancellationToken cancellationToken)
        {
            await _underlyingDispatcherAccessor().DispatchAsync(message as IEvent, cancellationToken);
        }

        private async Task OnRecievedCommand(object message, Func<object, Exception, object> faultMessageCreator, CancellationToken cancellationToken)
        {
            try
            {
                await _commandProcessor.ProcessAsync(message as ICommand, cancellationToken);
            }
            catch (Exception ex)
            {
                await _caraBus.PublishAsync(faultMessageCreator(message, ex), cancellationToken: cancellationToken);
            }
        }
    }
}