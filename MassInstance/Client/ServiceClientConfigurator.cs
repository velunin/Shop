using System;
using System.Collections.Generic;
using System.Linq;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Client
{
    internal class ServiceClientConfigurator : IServiceClientConfigurator
    {
        private readonly HashSet<Type> _serviceTypes = new HashSet<Type>();
        private readonly IQueuesMapperBuilder _mapperBuilder = new QueuesMapperBuilder();

        public void AddService<TService>() where TService : IServiceMap
        {
            _serviceTypes.Add(typeof(TService));
            _mapperBuilder.Add<TService>();
        }

        public IEnumerable<Type> GetServices()
        {
            return _serviceTypes.Select(x => x);
        }

        public IQueuesMapper BuildQueueMapper()
        {
            return _mapperBuilder.Build();
        }
    }
}