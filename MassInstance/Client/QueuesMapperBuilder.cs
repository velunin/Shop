using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MassInstance.Configuration.ServiceMap;
using MassInstance.Cqrs.Commands;

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
                var queueFields = serviceType
                    .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(f => f
                        .FieldType
                        .GetInterfaces()
                        .Any(i => i == typeof(IQueueMap)));

                foreach (var queueField in queueFields)
                {
                    var queueFieldType = queueField.FieldType;
                    var queueName = ServiceMapHelper.ExtractQueueName(queueField);

                    var commandFields = queueFieldType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(f => f
                            .FieldType
                            .GetInterfaces()
                            .Any(i => i == typeof(ICommand)));

                    foreach (var commandField in commandFields)
                    {
                        queuesMapper.Map(commandField.FieldType, queueName);
                    }
                }
            }

            return queuesMapper;
        }
    }
}