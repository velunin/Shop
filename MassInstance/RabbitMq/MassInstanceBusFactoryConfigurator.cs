using System;
using System.Collections.Generic;

using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;
using MassTransit;
using MassTransit.NewIdProviders;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.RabbitMqTransport.Configurators;

namespace MassInstance.RabbitMq
{
    public class MassInstanceBusFactoryConfigurator : RabbitMqBusFactoryConfigurator, IMassInstanceBusFactoryConfigurator
    {
        private readonly IMassInstanceConsumerFactory _consumerFactory;

        private readonly IDictionary<Type, ServiceConfigurationContext> _serviceConfigurations =
            new Dictionary<Type, ServiceConfigurationContext>();

        public MassInstanceBusFactoryConfigurator(
            IRabbitMqBusConfiguration configuration, 
            IRabbitMqEndpointConfiguration busEndpointConfiguration, 
            IMassInstanceConsumerFactory consumerFactory) 
            : base(configuration, busEndpointConfiguration)
        {
            _consumerFactory = consumerFactory;
        }

        public new IBusControl CreateBus()
        {
            foreach (var (serviceType,serviceConfigurationContext) in _serviceConfigurations)
            {
                var host = serviceConfigurationContext.Host;
                var serviceConfiguration = serviceConfigurationContext.Configuration;

                foreach (var queueInfo in ServiceMapHelper.ExtractQueues(serviceType))
                {
                    CreateQueue(serviceConfiguration, host, queueInfo.Name, queueInfo.Type);
                }
            }

            return base.CreateBus();
        }

        public IMassInstanceBusFactoryConfigurator AddService<TService>(
            IRabbitMqHost host,
            Action<IServiceConfiguration<TService>> configureService) where TService : IServiceMap
        {
            var serviceType = typeof(TService);

            if (_serviceConfigurations.ContainsKey(serviceType))
            {
                throw new ArgumentException($"Configuration for '{serviceType.Name}' already exist");
            }

            var serviceConfiguration = new ServiceConfiguration<TService>();

            configureService(serviceConfiguration);

            var serviceConfigurationContext = new ServiceConfigurationContext
            {

            };

            _serviceConfigurations.TryAdd(serviceType, new ServiceConfigurationContext
            {
                Host = host,
                Configuration = serviceConfiguration
            });

            return this;
        }

        private void CreateQueue(
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

                foreach (var commandInfo in ServiceMapHelper.ExtractCommands(queueType))
                {
                    CreateCommandConsumer(queueConfiguration, serviceConfiguration, endpointConfiguration, commandInfo.Type);
                }
            });
        }

        private void CreateCommandConsumer(
            IQueueConfiguration queueConfiguration,
            IServiceConfiguration serviceConfiguration,
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator,
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

            var consumerType = CommandConsumerTypeFactory.Create(commandType);
            if (!_consumerFactory.TryCreateConsumer(consumerType, out var commandConsumer))
            {
                return;
            }

            configureExceptionHandling?.Invoke(commandExceptionHandlingOptions);

            ExceptionResponseResolver.Map(commandType, commandExceptionHandlingOptions);

            endpointConfigurator.Consumer(consumerType, _ => commandConsumer);
        }

        private class ServiceConfigurationContext
        {
            public IRabbitMqHost Host { get; set; }

            public IServiceConfiguration Configuration { get; set; }
        }
    }
}