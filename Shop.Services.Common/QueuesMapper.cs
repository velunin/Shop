using System;
using System.Collections.Concurrent;

namespace Shop.Services.Common
{
    public class QueuesMapper : IQueuesMapper
    {
        private readonly ConcurrentDictionary<Type,string> _map = new ConcurrentDictionary<Type, string>();

        public IQueuesMapper Map<TCommandType>(string queueName)
        {
            if (!_map.TryAdd(typeof(TCommandType), queueName))
            {
                throw new InvalidOperationException($"For type {typeof(TCommandType)} map already registered");
            }

            return this;
        }

        public string GetQueueName(Type commandType)
        {
            return _map.TryGetValue(commandType, out var queueName) 
                ? queueName 
                : null;
        }

        public string GetQueueName<TCommandType>()
        {
            return GetQueueName(typeof(TCommandType));
        }
    }
}