using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MassInstance.Bus;
using MassInstance.Client;
using MassInstance.Configuration;
using MassInstance.Configuration.Client;
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
        private readonly ISagaMessageExtractor _sagaMessageExtractor;
        private readonly IServiceClientConfigurator _serviceClientConfigurator = new ServiceClientConfigurator();

        private readonly IDictionary<Type, IRabbitMqServiceConfiguration> _serviceConfigurations =
            new Dictionary<Type, IRabbitMqServiceConfiguration>();

        private Assembly[] _sagaStateMachineAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        private IRabbitMqHost _callbackHost;

        private string _callbackQueueName;

        public MassInstanceBusFactoryConfigurator(
            IRabbitMqBusConfiguration configuration, 
            IRabbitMqEndpointConfiguration busEndpointConfiguration, 
            IMassInstanceConsumerFactory consumerFactory, 
            ISagaMessageExtractor sagaMessageExtractor) 
            : base(configuration, busEndpointConfiguration)
        {
            _consumerFactory = consumerFactory;
            _sagaMessageExtractor = sagaMessageExtractor;
        }

        public new IServiceBus CreateBus()
        {
            CreateServiceConsumers();
            CreateCallbackConsumers();

            var serviceBus = new ServiceBus(
                base.CreateBus(),
                _serviceClientConfigurator.BuildQueueMapper(),
                new SerivceClientConfig
                {
                    BrokerUri = _callbackHost?.Address,
                    CallbackQueue = _callbackQueueName
                });

            return serviceBus;
        }

        public IMassInstanceBusFactoryConfigurator AddServiceHost<TService>(
            IRabbitMqHost host,
            Action<IServiceConfiguration<TService>> configureService) where TService : IServiceMap
        {
            var serviceType = typeof(TService);
            var serviceConfiguration = new RabbitMqServiceConfiguration<TService>(host);

            configureService(serviceConfiguration);

            _serviceConfigurations.Add(serviceType, serviceConfiguration);

            return this;
        }

        public void AddServiceClient(IRabbitMqHost callbackHost, string callbackQueue, Action<IServiceClientConfigurator> configureServiceClient)
        {
            if (string.IsNullOrEmpty(callbackQueue))
            {
                throw new ArgumentNullException(callbackQueue);
            }

            _callbackQueueName = callbackQueue;
            _callbackHost = callbackHost;

            configureServiceClient(_serviceClientConfigurator);
        }

        public Assembly[] SagaStateMachineAssemblies
        {
            set => _sagaStateMachineAssemblies = value;
        }

        private void CreateServiceConsumers()
        {
            foreach (var (serviceType, serviceConfiguration) in _serviceConfigurations)
            {
                var host = serviceConfiguration.Host;

                foreach (var queueInfo in ServiceMapHelper.ExtractQueues(serviceType))
                {
                    CreateQueue(
                        serviceConfiguration,
                        host,
                        queueInfo.Name,
                        queueInfo.Type);
                }
            }
        }

        private void CreateCallbackConsumers()
        {
            if (_callbackHost == null)
            {
                return;
            }

            var commandResultTypes = _serviceClientConfigurator
                .GetServices()
                .SelectMany(ServiceMapHelper.ExtractServiceCommandsResults);

            ReceiveEndpoint(_callbackHost, _callbackQueueName, endpointCfg =>
            {
                foreach (var resultType in commandResultTypes)
                {
                    var callbackConsumerType = CommandConsumerTypeFactory.CreateCallbackConsumer(resultType);
                    
                    endpointCfg.Consumer(
                        callbackConsumerType, 
                        _ => _consumerFactory.CreateConsumer(callbackConsumerType));
                }
            }); 
        }

        private void CreateQueue(
            IServiceConfiguration serviceConfiguration,
            IRabbitMqHost host, 
            string queueName, 
            Type queueType)
        {
            ReceiveEndpoint(host, queueName, endpointConfiguration =>
            {
                var sagaMessageTypes = new HashSet<Type>();

                if (serviceConfiguration.TryGetQueueConfig(queueName, out var queueConfiguration))
                {
                    foreach (var sagaInstanceType in queueConfiguration.GetSagaInstanceTypes())
                    {
                        _consumerFactory.CreateSaga(sagaInstanceType, endpointConfiguration);

                        sagaMessageTypes.UnionWith(
                            _sagaMessageExtractor.Extract(
                                sagaInstanceType, 
                                _sagaStateMachineAssemblies));
                    }
                }

                //Retrieve commands from service map with excluding saga event types
                var commands = ServiceMapHelper
                    .ExtractCommands(queueType)
                    .Where(command => !sagaMessageTypes.Contains(command.Type)); 

                foreach (var commandInfo in commands)
                {
                    CreateCommandConsumer(
                        queueConfiguration,
                        serviceConfiguration,
                        endpointConfiguration,
                        commandInfo.Type);
                }
            });
        }

        private void CreateCommandConsumer(
            IQueueConfiguration queueConfiguration,
            IServiceConfiguration serviceConfiguration,
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator,
            Type commandType)
        {
            var commandExceptionHandlingOptions = new CommandExceptionHandlingOptions();

            var configureExceptionHandling = 
                serviceConfiguration.ConfigureCommandExceptionHandling + queueConfiguration.ConfigureCommandExceptionHandling;

            if (queueConfiguration.TryGetCommandConfig(commandType, out var commandConfiguration))
            {
                configureExceptionHandling += commandConfiguration.ConfigureExceptionHandling;
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
    }
}