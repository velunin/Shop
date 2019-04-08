using System;
using System.Collections.Generic;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Client
{
    public class QueuesMapperBuilder : IQueuesMapperBuilder
    {
        private readonly HashSet<Type> _serviceTypes = new HashSet<Type>();

        public IQueuesMapperBuilder Add<TService>() where TService : IServiceMap
        {
            _serviceTypes.Add(typeof(TService));

            return this;
        }

        public IQueuesMapper Build()
        {
            var queuesMapper = new QueuesMapper();

            foreach (var serviceType in _serviceTypes)
            {
                foreach (var queueInfo in ServiceMapHelper.ExtractQueues(serviceType))
                {
                    foreach (var commandInfo in ServiceMapHelper.ExtractCommands(queueInfo.Type))
                    {
                        queuesMapper.Map(commandInfo.Type, queueInfo.Name);
                    }
                }
            }

            return queuesMapper;
        }
    }
}