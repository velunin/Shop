using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MassInstance.Cqrs.Commands;
using MassTransit;

namespace MassInstance.Configuration
{
    public class QueueConfiguratorBase : IQueueConfiguratorBuilder
    {
        private readonly Type _queueType;
        private readonly IMassInstanceConsumerFactory _consumerFactory;
        private readonly IReceiveEndpointConfigurator _receiveEndpointConfigurator;

        protected readonly IDictionary<Type,Action<CommandExceptionHandlingOptions>> CommandExceptionHanlingConfigActions = 
            new Dictionary<Type, Action<CommandExceptionHandlingOptions>>();

        public QueueConfiguratorBase(
            IMassInstanceConsumerFactory massInstanceConsumerFactory, 
            IReceiveEndpointConfigurator endpointConfigurator, 
            Type queueType)
        {
            _consumerFactory = massInstanceConsumerFactory ?? throw new ArgumentNullException(nameof(massInstanceConsumerFactory));
            _queueType = queueType ?? throw new ArgumentNullException(nameof(queueType));
            _receiveEndpointConfigurator = endpointConfigurator ?? throw new ArgumentNullException(nameof(endpointConfigurator));
        }

        public Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        public void Build()
        {
            var commandFields = _queueType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(ICommand)));

            foreach (var commandField in commandFields)
            {
                var commandType = commandField.FieldType;
                var consumerType = ConfigurationHelper.CreateConsumerTypeByCommandType(commandType);

                var commandExceptionHandling = new CommandExceptionHandlingOptions();

                ConfigureCommandExceptionHandling?.Invoke(commandExceptionHandling);

                ExceptionResponseResolver.Map(commandType, commandExceptionHandling);

                _receiveEndpointConfigurator.Consumer(
                    consumerType, 
                    type => _consumerFactory.CreateConsumer(consumerType));
            }
        }
    }
}