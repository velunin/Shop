using System;
using System.Collections.Generic;

using MassInstance.Configuration.ServiceMap;
using MassTransit.RabbitMqTransport;

namespace MassInstance.Configuration
{
    public class CompositionServiceConfigurator : IRabbitMqBusCompositionServiceConfigurator, ICompositionServiceConfiguration
    {
        private readonly IMassInstanceConsumerFactory _massInstanceConsumerFactory;

        private readonly IDictionary<Type, (IRabbitMqBusServiceConfigurator, Action<CommandExceptionHandlingOptions>)>
            _serviceConfigurators =
                new Dictionary<Type, (IRabbitMqBusServiceConfigurator, Action<CommandExceptionHandlingOptions>)>();

        public CompositionServiceConfigurator(
            IMassInstanceConsumerFactory massInstanceConsumerFactory)
        {
            _massInstanceConsumerFactory =
                massInstanceConsumerFactory ?? throw new ArgumentNullException(nameof(massInstanceConsumerFactory));
        }

        public void AddService<TService>(
            Action<IServiceConfiguration<TService>> configureService = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) 
            where TService : IServiceMap
        {
            var serviceType = typeof(TService);

            if (_serviceConfigurators.ContainsKey(serviceType))
            {
                throw new ArgumentException($"Configuration for '{serviceType}' already exist");
            }

            var serviceConfig = new ServiceConfigurator<TService>(
                _massInstanceConsumerFactory);

            configureService?.Invoke(serviceConfig);

            _serviceConfigurators.Add(
                serviceType,
                (serviceConfig, configureExceptionHandling));
        }

        public void Build(
            IRabbitMqBusFactoryConfigurator busConfigurator,
            IRabbitMqHost host,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            foreach (var serviceConfiguratorEntry in _serviceConfigurators)
            {
                var (serviceConfiguratorItem, configureExceptionHanlingForService) = serviceConfiguratorEntry.Value;
                
                serviceConfiguratorItem
                    .Build(
                        busConfigurator,
                        host,
                        configureExceptionHandling + configureExceptionHanlingForService);
            }
        }
    }
}