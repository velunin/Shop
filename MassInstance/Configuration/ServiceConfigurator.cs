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
    public class ServiceConfiguratorBuilder<TService> : IConfiguratorBuilder, IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly IMassInstanceConsumerFactory _consumerFactory;
        private readonly IMassInstanceBusFactoryConfigurator _busConfigurator;
        private readonly IRabbitMqHost _host;

        private readonly IDictionary<string, QueueConfiguratorBase> _queuesConfigsOverrides =
            new Dictionary<string, QueueConfiguratorBase>();

        private readonly IDictionary<Type,Type> _sagaQueues = new Dictionary<Type, Type>();

        public ServiceConfiguratorBuilder(
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
            Action<IQueueConfiguration<TQueue>> configureQueue = null) where TQueue : IQueueMap
        {
            var queueConfiguration = new QueueConfigurator<TQueue>(_consumerFactory);
            queueConfiguration.ConfigureCommandExceptionHandling += ConfigureCommandExceptionHandling;

            var queueName = ((MemberExpression) queueSelector.Body).Member.Name;

            configureQueue?.Invoke(queueConfiguration);

            _queuesConfigsOverrides.Add(queueName, queueConfiguration);
        }

        public void ConfigureAsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector, Type sagaInstanceType) 
            where TQueue : IQueueMap 
        {
            _sagaQueues.Add(queueSelector.ReturnType, sagaInstanceType);
        }

        public Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        public void Build()
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

                _busConfigurator.ReceiveEndpoint(_host, ServiceMapHelper.ExtractQueueName(queueField), e =>
                {
                    var queueConfigurator = new QueueConfiguratorBase(_consumerFactory, e, queueType);

                    if (_queuesConfigsOverrides.TryGetValue(
                        queueField.Name,
                        out var queueConfiguratorOverride))
                    {
                       queueConfiguratorOverride.Build();
                    }
                    else
                    {
                        queueConfigurator.Build();
                    }
                });
            }
        }

    }

    public class ServiceConfiguration<TService> : IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly IDictionary<Type, IQueueConfiguration> _queuesConfigurations =
            new Dictionary<Type, IQueueConfiguration>();

        private readonly IDictionary<Type, Type> _sagaQueues = new Dictionary<Type, Type>();

        public void Configure<TServiceMap, TQueue>(
            Expression<Func<TServiceMap, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null)
            where TServiceMap : IServiceMap
            where TQueue : IQueueMap
        {
            Validate(typeof(TQueue));

            Configure(typeof(TQueue), configureQueue);
        }

        public void ConfigureAsSaga<TServiceMap, TQueue>(Expression<Func<TServiceMap, TQueue>> queueSelector,
            Type sagaInstanceType)
            where TServiceMap : IServiceMap
            where TQueue : IQueueMap
        {
            Validate(typeof(TQueue));

            _sagaQueues.Add(typeof(TQueue), sagaInstanceType);
        }

        public IQueueConfiguration GetConfigurationForQueue(Type queueType)
        {
            throw new NotImplementedException();
        }

        public Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        public void Configure<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null)
            where TQueue : IQueueMap
        {
            Validate(typeof(TQueue));

            Configure(typeof(TQueue), configureQueue);
        }

        public void ConfigureAsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector, Type sagaInstanceType)
            where TQueue : IQueueMap
        {
            Validate(typeof(TQueue));

            _sagaQueues.Add(typeof(TQueue), sagaInstanceType);
        }

        private void Configure<TQueue>(Type queueType, Action<IQueueConfiguration<TQueue>> configureQueue = null) where TQueue : IQueueMap
        {
            Validate(queueType);

            var queueConfiguration = new QueueConfiguration<TQueue>();
            configureQueue?.Invoke(queueConfiguration);
            queueConfiguration.ConfigureCommandExceptionHandling =
                ConfigureCommandExceptionHandling +
                queueConfiguration.ConfigureCommandExceptionHandling;

            _queuesConfigurations.Add(queueType, queueConfiguration);
        }

        private void Validate(Type queueType)
        {
            if (_queuesConfigurations.ContainsKey(queueType))
            {
                throw new InvalidOperationException($"Queue {queueType} already configured");
            }
        }
    }
}