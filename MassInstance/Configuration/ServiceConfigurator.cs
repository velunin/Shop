using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MassInstance.Configuration.ServiceMap;
using MassInstance.RabbitMq;
using MassTransit.RabbitMqTransport;

namespace MassInstance.Configuration
{
    public class ServiceConfigurator<TService> : IRabbitMqBusServiceConfigurator, IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly IMassInstanceConsumerFactory _consumerFactory;
        private readonly IMassInstanceBusFactoryConfigurator _busConfigurator;
        private readonly IRabbitMqHost _host;

        private readonly IDictionary<string, (QueueConfiguratorBase, Action<CommandExceptionHandlingOptions>)> _queuesConfigsOverrides =
            new Dictionary<string, (QueueConfiguratorBase, Action<CommandExceptionHandlingOptions>)>();

        private readonly IDictionary<Type,Type> _sagaQueues = new Dictionary<Type, Type>();

        public ServiceConfigurator(
            IMassInstanceBusFactoryConfigurator busConfigurator,
            IMassInstanceConsumerFactory massInstanceConsumerFactory,
            IRabbitMqHost host)
        {
            _consumerFactory =
                massInstanceConsumerFactory ?? throw new ArgumentNullException(nameof(massInstanceConsumerFactory));
            _busConfigurator = busConfigurator ?? throw new ArgumentNullException(nameof(busConfigurator));
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public void Configure<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TQueue : IQueueMap
        {
            var queueConfiguration = new QueueConfigurator<TQueue>(_consumerFactory);
            var queueName = ((MemberExpression) queueSelector.Body).Member.Name;

            configureQueue?.Invoke(queueConfiguration);

            _queuesConfigsOverrides.Add(queueName, (queueConfiguration,configureExceptionHandling));
        }

        public void ConfigureAsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector, Type sagaInstanceType) 
            where TQueue : IQueueMap 
        {
            _sagaQueues.Add(queueSelector.ReturnType, sagaInstanceType);
        }

        public void Build(Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            var serviceType = typeof(TService);
            var queueFields = serviceType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(IQueueMap)));

            foreach (var queueField in queueFields)
            {
                var queueType = queueField.FieldType;

                if (_sagaQueues.TryGetValue(queueType, out var sagaType))
                {
                    _busConfigurator.ReceiveEndpoint(_host, ServiceMapHelper.ExtractQueueName(queueField), e =>
                    {
                        _consumerFactory.CreateSaga(sagaType, e);
                    });
                    continue;
                }

                var queueConfigurator = new QueueConfiguratorBase(_consumerFactory, queueType);
                var composeConfigureHandlingActions = configureExceptionHandling;

                if (_queuesConfigsOverrides.TryGetValue(
                    queueField.Name, 
                    out var queueConfigOverrideEntry))
                {
                    var (queueConfiguratorOverride, commandExceptionHandlingForQueue) = queueConfigOverrideEntry;
                    queueConfigurator = queueConfiguratorOverride;

                    composeConfigureHandlingActions += commandExceptionHandlingForQueue;
                }
                
                _busConfigurator.ReceiveEndpoint(_host, ServiceMapHelper.ExtractQueueName(queueField), e =>
                {
                    queueConfigurator.Configure(e, composeConfigureHandlingActions);
                });
            }
        }

    }
}