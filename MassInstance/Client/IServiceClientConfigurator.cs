using System;
using System.Collections.Generic;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Client
{
    public interface IServiceClientConfigurator
    {
        void AddService<TService>() where TService : IServiceMap;

        IEnumerable<Type> GetServices();

        IQueuesMapper BuildQueueMapper();
    }
}