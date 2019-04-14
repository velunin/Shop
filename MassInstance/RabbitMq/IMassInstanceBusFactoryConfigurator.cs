using System;
using System.Reflection;
using MassInstance.Client;
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

        void AddServiceClient(
            IRabbitMqHost callbackHost, 
            string callbackQueue,
            Action<IServiceClientConfigurator> configureServiceClient);

        Assembly[] SagaStateMachineAssemblies { set; }
    }
}