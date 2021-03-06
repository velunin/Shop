﻿using System;
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
using EventInfo = MassInstance.Configuration.ServiceMap.EventInfo;

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

        private bool _isConfiguredAsHost;
        private bool _isConfiguredAsClient;

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

            _isConfiguredAsHost = true;

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

            _isConfiguredAsClient = true;
        }

        public Assembly[] SagaStateMachineAssemblies
        {
            set => _sagaStateMachineAssemblies = value;
        }

        private void CreateServiceConsumers()
        {
            if(!_isConfiguredAsHost) return;
            
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
            if (!_isConfiguredAsClient) return;

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

                //Retrieve commands and events from service map with excluding saga event types
                var commands = ServiceMapHelper
                    .ExtractCommands(queueType)
                    .Where(command => !sagaMessageTypes.Contains(command.Type));

                var events = ServiceMapHelper
                    .ExtractEvents(queueType)
                    .Where(@event => !sagaMessageTypes.Contains(@event.Type));

                foreach (var commandInfo in commands)
                {
                    CreateCommandConsumer(
                        queueConfiguration,
                        serviceConfiguration,
                        endpointConfiguration,
                        commandInfo.Type);
                }

                foreach (var eventInfo in events)
                {
                    CreateEventConsumer(
                        eventInfo, 
                        endpointConfiguration);
                }
            });
        }

        private void CreateEventConsumer(EventInfo eventInfo, IReceiveEndpointConfigurator endpointConfiguration)
        {
            var eventConsumerType = CommandConsumerTypeFactory.CreateEventConsumer(eventInfo.Type);
            if (!_consumerFactory.TryCreateConsumer(eventConsumerType, out var eventConsumer))
            {
                return;
            }

            endpointConfiguration.Consumer(eventConsumerType, _ => eventConsumer);
        }

        private void CreateCommandConsumer(
            IQueueConfiguration queueConfiguration,
            IServiceConfiguration serviceConfiguration,
            IReceiveEndpointConfigurator endpointConfigurator,
            Type commandType)
        {
            var commandExceptionHandlingOptions = new CommandExceptionHandlingOptions();

            var configureExceptionHandling = 
                serviceConfiguration.ConfigureCommandExceptionHandling + 
                queueConfiguration.ConfigureCommandExceptionHandling;

            if (queueConfiguration.TryGetCommandConfig(commandType, out var commandConfiguration))
            {
                configureExceptionHandling += commandConfiguration.ConfigureExceptionHandling;
            }

            var consumerType = CommandConsumerTypeFactory.CreateCommandConsumer(commandType);
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