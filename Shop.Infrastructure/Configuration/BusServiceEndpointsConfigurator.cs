﻿using System;
using System.Collections.Generic;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Shop.Infrastructure.Messaging.Extensions;

namespace Shop.Infrastructure.Configuration
{
    public class BusServiceEndpointsConfigurator : IBusServiceEndpointsConfigurator
    {
        private readonly List<EndpointConfigItem> _configs = new List<EndpointConfigItem>();
        private readonly IServiceCollection _serviceCollection;
        private readonly IConsumerCacheService _consumerCacheService;

        public BusServiceEndpointsConfigurator(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _consumerCacheService = serviceCollection
                .BuildServiceProvider()
                .GetService<IConsumerCacheService>();
        }

        public IBusServiceEndpointsConfigurator AddServiceEndpoint(
            string queueName, 
            Action<IBusServiceConfigurator> configAction, 
            Action<CommandExceptionHandlingOptions> exceptionHandlingConfig = null,
            Action<IReceiveEndpointConfigurator> receiveEndpointConfigure = null)
        {
            var configurator = new BusServiceConfigurator(_serviceCollection, _consumerCacheService);

            configAction(configurator);

            _configs.Add(new EndpointConfigItem
            {
                QueueName = queueName,
                ReceiveEndpointConfigurator = receiveEndpointConfigure,
                EndpointExceptionHandlingConfig = exceptionHandlingConfig,
                ServiceConfigurator = configurator
            });

            return this;
        }

        public void Configure(IRabbitMqBusFactoryConfigurator busFactoryConfigurator, IServiceProvider provider, IRabbitMqHost host)
        {
            foreach (var endpointConfigItem in _configs)
            {
                busFactoryConfigurator.ReceiveEndpoint(host, endpointConfigItem.QueueName, e =>
                {
                    e.UseCommandExceptionHandling(endpointConfigItem.EndpointExceptionHandlingConfig);

                    endpointConfigItem.ReceiveEndpointConfigurator?.Invoke(e);

                    endpointConfigItem.ServiceConfigurator.Configure(e, provider);
                });
            }
        }

        public class EndpointConfigItem
        {
            public string QueueName { get; set; }

            public Action<IReceiveEndpointConfigurator> ReceiveEndpointConfigurator { get; set; }

            public Action<CommandExceptionHandlingOptions> EndpointExceptionHandlingConfig { get; set; }

            public IBusServiceConfigurator ServiceConfigurator { get; set; }
        }
    }
}