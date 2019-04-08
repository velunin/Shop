using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public class ServiceConfiguration<TService> : IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly IDictionary<string, IQueueConfiguration> _queuesConfigurations =
            new Dictionary<string, IQueueConfiguration>();

        public void Configure<TServiceMap, TQueue>(
            Expression<Func<TServiceMap, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null)
            where TServiceMap : IServiceMap
            where TQueue : IQueueMap
        {
            var queueField = ((MemberExpression)queueSelector.Body).Member;

            Configure(ServiceMapHelper.ExtractQueueName(queueField), configureQueue);
        }

        public bool TryGetQueueConfig(string queueName, out IQueueConfiguration queueConfiguration)
        {
            return _queuesConfigurations.TryGetValue(queueName, out queueConfiguration);
        }

        public Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        public void Configure<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null)
            where TQueue : IQueueMap
        {
            var queueField = ((MemberExpression) queueSelector.Body).Member;

            Configure(ServiceMapHelper.ExtractQueueName(queueField), configureQueue);
        }

        private void Configure<TQueue>(string queueName, Action<IQueueConfiguration<TQueue>> configureQueue = null) where TQueue : IQueueMap
        {
            Validate(queueName);

            var queueConfiguration = new QueueConfiguration<TQueue>();
            configureQueue?.Invoke(queueConfiguration);
            queueConfiguration.ConfigureCommandExceptionHandling =
                ConfigureCommandExceptionHandling +
                queueConfiguration.ConfigureCommandExceptionHandling;

            _queuesConfigurations.Add(queueName, queueConfiguration);
        }

        private void Validate(string queueName)
        {
            if (_queuesConfigurations.ContainsKey(queueName))
            {
                throw new InvalidOperationException($"Queue '{queueName}' already configured");
            }
        }
    }
}