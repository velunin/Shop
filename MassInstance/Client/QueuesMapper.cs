using System;
using System.Collections.Concurrent;

namespace MassInstance.Client
{
    public class QueuesMapper : IQueuesMapper
    {
        private readonly ConcurrentDictionary<Type,string> _map = new ConcurrentDictionary<Type, string>();

        public IQueuesMapper Map<TCommandType>(string queueName)
        {
            return Map(typeof(TCommandType), queueName);
        }

        public IQueuesMapper Map(Type commandType, string queueName)
        {
            if (!_map.TryAdd(commandType, queueName))
            {
                throw new InvalidOperationException($"For type {commandType} map already registered");
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