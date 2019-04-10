using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.RabbitMqTransport.Configurators;

namespace MassInstance.RabbitMq
{
    public class MassInstanceBusFactoryConfigurator : RabbitMqBusFactoryConfigurator, IMassInstanceBusFactoryConfigurator
    {
        private readonly IMassInstanceConsumerFactory _consumerFactory;

        private readonly IDictionary<Type, IRabbitMqServiceConfiguration> _serviceConfigurations =
            new Dictionary<Type, IRabbitMqServiceConfiguration>();

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
            foreach (var (serviceType,serviceConfiguration) in _serviceConfigurations)
            {
                var host = serviceConfiguration.Host;

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
            var serviceConfiguration = new ServiceConfiguration<TService>();

            configureService(serviceConfiguration);

            _serviceConfigurations.Add(serviceType, new RabbitMqServiceConfiguration(host,serviceConfiguration));

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

    public interface IRabbitMqServiceConfiguration : IServiceConfiguration
    {
        IRabbitMqHost Host { get; }
    }

    public class RabbitMqServiceConfiguration : IRabbitMqServiceConfiguration
    {
        private readonly IServiceConfiguration _serviceConfiguration;

        public RabbitMqServiceConfiguration(IRabbitMqHost host, IServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration ?? throw new ArgumentNullException(nameof(serviceConfiguration));

            Host = host ?? throw new ArgumentNullException(nameof(host));

            ConfigureCommandExceptionHandling = _serviceConfiguration.ConfigureCommandExceptionHandling;
        }

        public void Configure<TService, TQueue>(
            Expression<Func<TService, TQueue>> queueSelector, 
            Action<IQueueConfiguration<TQueue>> configureQueue = null) 
            where TService : IServiceMap where TQueue : IQueueMap
        {
            _serviceConfiguration.Configure(queueSelector, configureQueue);
        }

        public bool TryGetQueueConfig(string queueName, out IQueueConfiguration queueConfiguration)
        {
            return _serviceConfiguration.TryGetQueueConfig(queueName, out queueConfiguration);
        }

        public Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        public IRabbitMqHost Host { get; }
    }
}