using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using MassTransit.RabbitMqTransport;
using Shop.Services.Common;

namespace Shop.Infrastructure.Configuration
{
    public class ServiceConfigurator<TService> : IRabbitMqBusServiceConfigurator, IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly ICommandConsumerFactory _commandConsumerFactory;
        private readonly IExceptionResponseResolver _exceptionResponseResolver;

        private readonly IDictionary<string, (QueueConfiguratorBase, Action<CommandExceptionHandlingOptions>)> _queuesConfigsOverrides =
            new Dictionary<string, (QueueConfiguratorBase, Action<CommandExceptionHandlingOptions>)>();

        private readonly HashSet<Type> _sagaTypes = new HashSet<Type>();

        public ServiceConfigurator(ICommandConsumerFactory commandConsumerFactory, IExceptionResponseResolver exceptionResponseResolver)
        {
            _commandConsumerFactory =
                commandConsumerFactory ?? throw new ArgumentNullException(nameof(commandConsumerFactory));

            _exceptionResponseResolver = 
                exceptionResponseResolver ?? throw new ArgumentNullException(nameof(exceptionResponseResolver));
        }

        public void ForQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TQueue : IQueueMap
        {
            var queueConfiguration = new QueueConfigurator<TQueue>(_commandConsumerFactory, _exceptionResponseResolver);
            var queueName = ((MemberExpression) queueSelector.Body).Member.Name;

            configureQueue?.Invoke(queueConfiguration);

            _queuesConfigsOverrides.Add(queueName, (queueConfiguration,configureExceptionHandling));
        }

        public void SetAsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector) where TQueue : IQueueMap
        {
            _sagaTypes.Add(queueSelector.ReturnType);
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

            var composeConfigureHandlingActions = configureExceptionHandling;

            foreach (var queueField in queueFields)
            {
                if (_sagaTypes.Contains(queueField.FieldType))
                {
                    continue;
                }

                var queueConfigurator = new QueueConfiguratorBase(_commandConsumerFactory, _exceptionResponseResolver, queueField.FieldType);

                if (_queuesConfigsOverrides.TryGetValue(
                    queueField.Name, 
                    out var queueConfigOverrideEntry))
                {
                    var (queueConfiguratorOverride, commandExceptionHandlingForQueue) = queueConfigOverrideEntry;
                    queueConfigurator = queueConfiguratorOverride;

                    composeConfigureHandlingActions += commandExceptionHandlingForQueue;
                }
                
                busConfigurator.ReceiveEndpoint(host, ExtractQueueName(queueField), e =>
                {
                    queueConfigurator.Configure(e, composeConfigureHandlingActions);
                });
            }
        }

        private static string ExtractQueueName(MemberInfo fieldInfo)
        {
            return Regex.Replace(
                    fieldInfo.Name,
                    "([A-Z])", "-$0",
                    RegexOptions.Compiled)
                .Trim('-')
                .ToLower();
        }
    }
}