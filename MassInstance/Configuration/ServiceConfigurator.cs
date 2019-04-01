using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MassInstance.Configuration.ServiceMap;
using MassTransit.RabbitMqTransport;

namespace MassInstance.Configuration
{
    public class ServiceConfigurator<TService> : IRabbitMqBusServiceConfigurator, IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly ICommandConsumerFactory _commandConsumerFactory;
        private readonly IExceptionResponseResolver _exceptionResponseResolver;
        private readonly ISagaConfigurator _sagaConfigurator;

        private readonly IDictionary<string, (QueueConfiguratorBase, Action<CommandExceptionHandlingOptions>)> _queuesConfigsOverrides =
            new Dictionary<string, (QueueConfiguratorBase, Action<CommandExceptionHandlingOptions>)>();

        private readonly IDictionary<Type,Type> _sagaQueues = new Dictionary<Type, Type>();

        public ServiceConfigurator(
            ICommandConsumerFactory commandConsumerFactory, 
            IExceptionResponseResolver exceptionResponseResolver, 
            ISagaConfigurator sagaConfigurator)
        {
            _commandConsumerFactory =
                commandConsumerFactory ?? throw new ArgumentNullException(nameof(commandConsumerFactory));

            _exceptionResponseResolver = 
                exceptionResponseResolver ?? throw new ArgumentNullException(nameof(exceptionResponseResolver));

            _sagaConfigurator = sagaConfigurator ?? throw new ArgumentNullException(nameof(sagaConfigurator));
        }

        public void Configure<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TQueue : IQueueMap
        {
            var queueConfiguration = new QueueConfigurator<TQueue>(_commandConsumerFactory, _exceptionResponseResolver);
            var queueName = ((MemberExpression) queueSelector.Body).Member.Name;

            configureQueue?.Invoke(queueConfiguration);

            _queuesConfigsOverrides.Add(queueName, (queueConfiguration,configureExceptionHandling));
        }

        public void ConfigureAsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector, Type sagaInstanceType) 
            where TQueue : IQueueMap 
        {
            _sagaQueues.Add(queueSelector.ReturnType, sagaInstanceType);
        }

        public void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator, 
            IRabbitMqHost host,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            var serviceType = typeof(TService);
            var queueFields = serviceType
                .GetFields(BindingFlags.Public)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(IQueueMap)));

            foreach (var queueField in queueFields)
            {
                var queueType = queueField.FieldType;

                if (_sagaQueues.TryGetValue(queueType, out var sagaType))
                {
                    busConfigurator.ReceiveEndpoint(host, ServiceMapHelper.ExtractQueueName(queueField), e =>
                    {
                        _sagaConfigurator.Configure(sagaType, e);
                    });
                    continue;
                }

                var queueConfigurator = new QueueConfiguratorBase(_commandConsumerFactory, _exceptionResponseResolver, queueType);
                var composeConfigureHandlingActions = configureExceptionHandling;

                if (_queuesConfigsOverrides.TryGetValue(
                    queueField.Name, 
                    out var queueConfigOverrideEntry))
                {
                    var (queueConfiguratorOverride, commandExceptionHandlingForQueue) = queueConfigOverrideEntry;
                    queueConfigurator = queueConfiguratorOverride;

                    composeConfigureHandlingActions += commandExceptionHandlingForQueue;
                }
                
                busConfigurator.ReceiveEndpoint(host, ServiceMapHelper.ExtractQueueName(queueField), e =>
                {
                    queueConfigurator.Configure(e, composeConfigureHandlingActions);
                });
            }
        }

    }
}