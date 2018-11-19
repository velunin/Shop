using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;
using Shop.Infrastructure.Messaging;
using Shop.Services.Common.MessageContracts;

namespace Shop.Infrastructure.Configuration
{
    public class BusServiceConfigurator : IBusServiceConfigurator
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IConsumerCacheService _consumerCacheService;
        private readonly List<Type> _eventTypes;

        private readonly IDictionary<Type, Action<CommandExceptionHandlingOptions>> _commandExceptionHandlingConfigs =
            new Dictionary<Type, Action<CommandExceptionHandlingOptions>>();

        public BusServiceConfigurator(IServiceCollection serviceCollection, IConsumerCacheService consumerCacheService)
        {
            _serviceCollection = serviceCollection;
            _consumerCacheService = consumerCacheService;
            _eventTypes = new List<Type>();
        }

        public IBusServiceConfigurator AddCommandConsumer<TCommand>(Action<CommandExceptionHandlingOptions> exceptionHandlingConfigure = null) where TCommand : ICommand
        {
            _commandExceptionHandlingConfigs.TryAdd(typeof(TCommand), exceptionHandlingConfigure);

            _serviceCollection.AddScoped(typeof(TCommand));

            var consumerTypes = CreateGenericCommandConsumersTypes(typeof(TCommand));

            foreach (var consumerType in consumerTypes)
            {
                _serviceCollection.AddScoped(consumerType);
            }

            return this;
        }

        public void Configure(IReceiveEndpointConfigurator endpointConfigurator, IServiceProvider provider)
        {
            foreach (var commandExceptionHandlingConfig in _commandExceptionHandlingConfigs)
            {
                endpointConfigurator.CommandConsumer(
                    commandExceptionHandlingConfig.Key, 
                    provider, 
                    commandExceptionHandlingConfig.Value);
            }

            foreach (var type in _eventTypes)
            {
                endpointConfigurator.EventConsumer(type,provider);
            }
        }

        public IEnumerable<(Type, Action<CommandExceptionHandlingOptions>)> GetCommandConsumerTypesWithConfigs()
        {
            return _commandExceptionHandlingConfigs
                .Select(commandExceptionHandlingConfig =>
                    (commandExceptionHandlingConfig.Key, commandExceptionHandlingConfig.Value));
        }

        public IBusServiceConfigurator AddEventConsumer<TEvent>() where TEvent : class,IEvent
        {
            var eventConsumerType = typeof(EventConsumer<>).MakeGenericType(typeof(TEvent));

            _eventTypes.Add(typeof(TEvent));
            _serviceCollection.AddScoped(eventConsumerType);

            return this;
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
    }
}