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

        public Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        public IQueueConfiguration<TQueue> SelectQueue<TServiceMap, TQueue>(
            Expression<Func<TServiceMap, TQueue>> queueSelector)
            where TServiceMap : IServiceMap
            where TQueue : IQueueMap
        {
            var queueField = ((MemberExpression)queueSelector.Body).Member;

            return GetOrCreateConfiguration<TQueue>(ServiceMapHelper.ExtractQueueName(queueField));
        }

        public IQueueConfiguration<TQueue> SelectQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector)
            where TQueue : IQueueMap
        {
            var queueField = ((MemberExpression)queueSelector.Body).Member;

            return GetOrCreateConfiguration<TQueue>(ServiceMapHelper.ExtractQueueName(queueField));
        }

        public bool TryGetQueueConfig(string queueName, out IQueueConfiguration queueConfiguration)
        {
            return _queuesConfigurations.TryGetValue(queueName, out queueConfiguration);
        }

        private IQueueConfiguration<TQueue> GetOrCreateConfiguration<TQueue>(string queueName) where TQueue : IQueueMap
        {
            Validate(queueName);

            if (_queuesConfigurations.ContainsKey(queueName))
            {
                return (IQueueConfiguration<TQueue>)_queuesConfigurations[queueName];
            }

            var queueConfiguration = new QueueConfiguration<TQueue>();

            _queuesConfigurations.Add(queueName, queueConfiguration);
            
            return queueConfiguration;
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