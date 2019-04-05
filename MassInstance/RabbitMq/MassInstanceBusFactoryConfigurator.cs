using System;
using System.Collections.Generic;
using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.RabbitMqTransport.Configurators;

namespace MassInstance.RabbitMq
{
    public class MassInstanceBusFactoryConfigurator : RabbitMqBusFactoryConfigurator, IMassInstanceBusFactoryConfigurator
    {
        private readonly IMassInstanceConsumerFactory _consumerFactory;

        private readonly IDictionary<Type, (IRabbitMqBusServiceConfigurator, Action<CommandExceptionHandlingOptions>)>
            _serviceConfigurators =
                new Dictionary<Type, (IRabbitMqBusServiceConfigurator, Action<CommandExceptionHandlingOptions>)>();

        public MassInstanceBusFactoryConfigurator(
            IRabbitMqBusConfiguration configuration, 
            IRabbitMqEndpointConfiguration busEndpointConfiguration, 
            IMassInstanceConsumerFactory consumerFactory) 
            : base(configuration, busEndpointConfiguration)
        {
            _consumerFactory = consumerFactory;
        }

        public IMassInstanceBusFactoryConfigurator AddService<TService>(
            IRabbitMqHost host,
            Action<IServiceConfiguration<TService>> configureService) where TService : IServiceMap
        {
            var serviceType = typeof(TService);

            if (_serviceConfigurators.ContainsKey(serviceType))
            {
                throw new ArgumentException($"Configuration for '{serviceType}' already exist");
            }

            var serviceConfig = new ServiceConfigurator<TService>(this,
                _consumerFactory,
                host);
            throw new NotImplementedException();
        }
    }
}