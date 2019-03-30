using System;
using System.Collections.Generic;
using MassInstance.Configuration.ServiceMap;
using MassTransit.RabbitMqTransport;

namespace MassInstance.Configuration
{
    public class CompositionServiceConfigurator : ICompositionServiceConfiguration, IRabbitMqBusCompositionServiceConfigurator
    {
        private readonly ICommandConsumerFactory _commandConsumerFactory;
        private readonly IExceptionResponseResolver _exceptionResponseResolver;

        private readonly IDictionary<Type, (IRabbitMqBusServiceConfigurator, Action<CommandExceptionHandlingOptions>)> _serviceConfigurators =
            new Dictionary<Type, (IRabbitMqBusServiceConfigurator, Action<CommandExceptionHandlingOptions>)>();

        public CompositionServiceConfigurator(
            ICommandConsumerFactory commandConsumerFactory, 
            IExceptionResponseResolver exceptionResponseResolver)
        {
            _commandConsumerFactory = commandConsumerFactory ?? throw new ArgumentNullException(nameof(commandConsumerFactory));
            _exceptionResponseResolver = exceptionResponseResolver ?? throw new ArgumentNullException(nameof(exceptionResponseResolver));
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

            var serviceConfig = new ServiceConfigurator<TService>(_commandConsumerFactory, _exceptionResponseResolver);

            configureService?.Invoke(serviceConfig);

            _serviceConfigurators.Add(
                serviceType,
                (serviceConfig, configureExceptionHandling));
        }

        public void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator,
            IRabbitMqHost host,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            foreach (var serviceConfiguratorEntry in _serviceConfigurators)
            {
                var (serviceConfiguratorItem, configureExceptionHanlingForService) = serviceConfiguratorEntry.Value;
                
                serviceConfiguratorItem
                    .Configure(
                        busConfigurator,
                        host,
                        configureExceptionHandling + configureExceptionHanlingForService);
            }
        }
    }
}