using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit.Saga;
using Microsoft.Extensions.DependencyInjection;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;
using Shop.Infrastructure.Messaging;
using Shop.Services.Common.MessageContracts;

namespace Shop.Infrastructure.Configuration
{
    public class BusServiceConfiguration : IBusServiceConfiguration
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly List<Type> _eventTypes;
        private readonly List<Type> _sagaTypes;

        private readonly IDictionary<Type, CommandConfigItem> _commandExceptionHandlingConfigs =
            new Dictionary<Type, CommandConfigItem>();

        public BusServiceConfiguration(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _eventTypes = new List<Type>();
            _sagaTypes = new List<Type>();
        }

        public BusServiceConfiguration()
        {
        }

        public IBusServiceConfiguration AddCommandConsumer<TCommand>(Action<CommandExceptionHandlingOptions> exceptionHandlingConfigure = null) where TCommand : ICommand
        {
            var commandType = typeof(TCommand);

            if (!_commandExceptionHandlingConfigs.TryAdd(commandType,
                new CommandConfigItem(commandType, exceptionHandlingConfigure)))
            {
                throw new InvalidOperationException($"Command with type {commandType.FullName} already configured");
            }

            _serviceCollection.AddScoped(typeof(TCommand));

            var consumerTypes = CreateGenericCommandConsumersTypes(typeof(TCommand));

            foreach (var consumerType in consumerTypes)
            {
                _serviceCollection.AddScoped(consumerType);
            }

            return this;
        }

        public IBusServiceConfiguration AddEventConsumer<TEvent>() where TEvent : class, IEvent
        {
            var eventConsumerType = typeof(EventConsumer<>).MakeGenericType(typeof(TEvent));

            _eventTypes.Add(typeof(TEvent));
            _serviceCollection.AddScoped(eventConsumerType);

            return this;
        }

        public IBusServiceConfiguration AddSaga<TSaga>() where TSaga : class, ISaga
        {
            _sagaTypes.Add(typeof(TSaga));

            return this;
        }

        public IEnumerable<CommandConfigItem> GetCommandConfigs()
        {
            return _commandExceptionHandlingConfigs.Values;
        }

        public IEnumerable<Type> GetEventsTypes()
        {
            return _eventTypes;
        }

        public IEnumerable<Type> GetSagasTypes()
        {
            return _sagaTypes;
        }

        private static IEnumerable<Type> CreateGenericCommandConsumersTypes(Type commandType)
        {
            var commandResultTypes = commandType
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResultingCommand<>))
                .Select(i => i.GetGenericArguments().FirstOrDefault())
                .ToList();

            var consumer = typeof(CommandRequestConsumer<,>);

            if (commandResultTypes.Any())
            {
                foreach (var commandResultType in commandResultTypes)
                {
                    yield return consumer.MakeGenericType(commandType, commandResultType);
                }
            }
            else
            {
                yield return consumer.MakeGenericType(commandType, typeof(EmptyResult));
            }
        }

        public class CommandConfigItem
        {
            public CommandConfigItem(
                Type messageType, 
                Action<CommandExceptionHandlingOptions> exceptionHandlingConfigure)
            {
                MessageType = messageType;
                ExceptionHandlingConfigure = exceptionHandlingConfigure;
            }

            public Type MessageType { get; }

            public Action<CommandExceptionHandlingOptions> ExceptionHandlingConfigure { get; }
        }

    }
}