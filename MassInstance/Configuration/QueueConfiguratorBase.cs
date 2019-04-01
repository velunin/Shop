using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MassInstance.Cqrs.Commands;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace MassInstance.Configuration
{
    public class QueueConfiguratorBase : IRabbitMqBusQueueConfigurator
    {
        private readonly Type _queueType;
        private readonly ICommandConsumerFactory _commandConsumerFactory;
        private readonly IExceptionResponseResolver _exceptionResponseResolver;

        protected readonly IDictionary<Type,Action<CommandExceptionHandlingOptions>> CommandExceptionHanlingConfigActions = 
            new Dictionary<Type, Action<CommandExceptionHandlingOptions>>();

        public QueueConfiguratorBase(ICommandConsumerFactory commandConsumerFactory, IExceptionResponseResolver exceptionResponseResolver, Type queueType)
        {
            _commandConsumerFactory = commandConsumerFactory ?? throw new ArgumentNullException(nameof(commandConsumerFactory));
            _exceptionResponseResolver = exceptionResponseResolver ?? throw new ArgumentNullException(nameof(exceptionResponseResolver));
            _queueType = queueType ?? throw new ArgumentNullException(nameof(queueType));
        }

        public void Configure(
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            var commandFields = _queueType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(ICommand)));

            var composeConfigureHandlingActions = configureExceptionHandling;

            foreach (var commandField in commandFields)
            {
                var commandType = commandField.FieldType;
                var consumerType = ConfigurationHelper.CreateConsumerTypeByCommandType(commandType);

                var commandExceptionHandling = new CommandExceptionHandlingOptions();

                if (CommandExceptionHanlingConfigActions.TryGetValue(
                    commandType,
                    out var configureExceptionHandlingForCommand))
                {
                    composeConfigureHandlingActions += configureExceptionHandlingForCommand;
                }

                composeConfigureHandlingActions?.Invoke(commandExceptionHandling);

                _exceptionResponseResolver.Map(commandType, commandExceptionHandling);

                endpointConfigurator.Consumer(
                    consumerType, 
                    type => _commandConsumerFactory.CreateConsumer(consumerType));
            }
        }
    }
}