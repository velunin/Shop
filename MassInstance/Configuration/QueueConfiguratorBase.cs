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
        private readonly IMassInstanceConsumerFactory _consumerFactory;

        protected readonly IDictionary<Type,Action<CommandExceptionHandlingOptions>> CommandExceptionHanlingConfigActions = 
            new Dictionary<Type, Action<CommandExceptionHandlingOptions>>();

        public QueueConfiguratorBase(IMassInstanceConsumerFactory massInstanceConsumerFactory, Type queueType)
        {
            _consumerFactory = massInstanceConsumerFactory ?? throw new ArgumentNullException(nameof(massInstanceConsumerFactory));
            _queueType = queueType ?? throw new ArgumentNullException(nameof(queueType));
        }

        public void Configure(
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            var commandFields = _queueType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
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

                ExceptionResponseResolver.Map(commandType, commandExceptionHandling);

                endpointConfigurator.Consumer(
                    consumerType, 
                    type => _consumerFactory.CreateConsumer(consumerType));
            }
        }
    }
}