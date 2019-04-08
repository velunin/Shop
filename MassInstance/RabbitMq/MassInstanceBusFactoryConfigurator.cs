using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;
using MassInstance.Cqrs.Commands;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.RabbitMqTransport.Configurators;

namespace MassInstance.RabbitMq
{
    public class MassInstanceBusFactoryConfigurator : RabbitMqBusFactoryConfigurator, IMassInstanceBusFactoryConfigurator
    {
        private readonly IMassInstanceConsumerFactory _consumerFactory;

        private readonly IDictionary<Type, (IConfiguratorBuilder, Action<CommandExceptionHandlingOptions>)>
            _serviceConfigurators =
                new Dictionary<Type, (IConfiguratorBuilder, Action<CommandExceptionHandlingOptions>)>();

        public MassInstanceBusFactoryConfigurator(
            IRabbitMqBusConfiguration configuration, 
            IRabbitMqEndpointConfiguration busEndpointConfiguration, 
            IMassInstanceConsumerFactory consumerFactory) 
            : base(configuration, busEndpointConfiguration)
        {
            _consumerFactory = consumerFactory;
        }

        public IMassInstanceBusFactoryConfigurator AddService<TService>(
            IRabbitMqHost host,
            Action<IServiceConfiguration<TService>> configureService) where TService : IServiceMap
        {
            var serviceType = typeof(TService);

            if (_serviceConfigurators.ContainsKey(serviceType))
            {
                throw new ArgumentException($"Configuration for '{serviceType}' already exist");
            }

            var serviceConfiguration = new ServiceConfiguration<TService>();

            configureService(serviceConfiguration);

            var queueFields = serviceType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(IQueueMap)));

            foreach (var queueField in queueFields)
            {
                var queueName = ServiceMapHelper.ExtractQueueName(queueField);
                var queueType = queueField.FieldType;

                SetupQueue(serviceConfiguration, host, queueName, queueType);
            }

            return this;
        }

        private void SetupQueue(
            IServiceConfiguration serviceConfiguration,
            IRabbitMqHost host, 
            string queueName, 
            Type queueType)
        {
            ReceiveEndpoint(host, queueName, endpointConfiguration =>
            {
                if (serviceConfiguration.TryGetQueueConfig(queueName, out var queueConfiguration))
                {
                    foreach (var sagaInstanceType in queueConfiguration.GetSagaInstanceTypes())
                    {
                        _consumerFactory.CreateSaga(sagaInstanceType, endpointConfiguration);
                    }
                }

                var commandFields = queueType
                    .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f
                        .FieldType
                        .GetInterfaces()
                        .Any(i => i == typeof(ICommand)));

                foreach (var commandField in commandFields)
                {
                    var commandType = commandField.FieldType;

                    SetupCommand(queueConfiguration, serviceConfiguration, commandType);
                }
            });
        }

        private void SetupCommand(
            IQueueConfiguration queueConfiguration,
            IServiceConfiguration serviceConfiguration,
            Type commandType)
        {
            var configureExceptionHandling = queueConfiguration.ConfigureCommandExceptionHandling ??
                                             serviceConfiguration.ConfigureCommandExceptionHandling;
            var commandExceptionHandlingOptions = new CommandExceptionHandlingOptions();

            if (queueConfiguration.TryGetCommandConfig(commandType, out var commandConfiguration))
            {
                configureExceptionHandling =
                    commandConfiguration.ConfigureExceptionHandling ?? configureExceptionHandling;
            }

            configureExceptionHandling?.Invoke(commandExceptionHandlingOptions);

            ExceptionResponseResolver.Map(commandType, commandExceptionHandlingOptions);

            _consumerFactory.CreateConsumer(
                ConfigurationHelper.CreateConsumerTypeByCommandType(commandType));
        }
    }
}