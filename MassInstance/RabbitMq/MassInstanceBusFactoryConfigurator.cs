using System;
using System.Collections.Generic;

using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;

using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.RabbitMqTransport.Configurators;

namespace MassInstance.RabbitMq
{
    public class MassInstanceBusFactoryConfigurator : RabbitMqBusFactoryConfigurator, IMassInstanceBusFactoryConfigurator
    {
        private readonly IMassInstanceConsumerFactory _consumerFactory;
        private readonly HashSet<Type> _serviceTypesHashSet = new HashSet<Type>();

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

            if (_serviceTypesHashSet.Contains(serviceType))
            {
                throw new ArgumentException($"Configuration for '{serviceType}' already exist");
            }

            _serviceTypesHashSet.Add(serviceType);

            var serviceConfiguration = new ServiceConfiguration<TService>();
            configureService(serviceConfiguration);

            foreach (var queueInfo in ServiceMapHelper.ExtractQueues(serviceType))
            {
                SetupQueue(serviceConfiguration, host, queueInfo.Name, queueInfo.Type);
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

                foreach (var commandInfo in ServiceMapHelper.ExtractCommands(queueType))
                {
                    SetupCommand(queueConfiguration, serviceConfiguration, commandInfo.Type);
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
                CommandConsumerTypeFactory.Create(commandType));
        }
    }
}