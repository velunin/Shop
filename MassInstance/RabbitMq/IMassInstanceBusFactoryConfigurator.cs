using System;
using System.Reflection;
using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;
using MassTransit.RabbitMqTransport;

namespace MassInstance.RabbitMq
{
    public interface IMassInstanceBusFactoryConfigurator : IRabbitMqBusFactoryConfigurator
    {
        IMassInstanceBusFactoryConfigurator AddServiceHost<TService>(
            IRabbitMqHost host,
            Action<IServiceConfiguration<TService>> configureService)
            where TService : IServiceMap;

        Assembly[] SagaStateMachineAssemblies { set; }
    }
}