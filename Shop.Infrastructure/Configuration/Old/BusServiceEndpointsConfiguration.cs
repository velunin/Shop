using System;
using System.Collections.Generic;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Shop.Infrastructure.Configuration
{
    public class BusServiceEndpointsConfiguration : IBusServiceEndpointsConfiguration
    {
        private readonly List<EndpointConfigItem> _configs = new List<EndpointConfigItem>();
        private readonly IServiceCollection _serviceCollection;

        public BusServiceEndpointsConfiguration(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public IBusServiceEndpointsConfiguration AddServiceEndpoint(
            string queueName, 
            Action<IBusServiceConfiguration> configAction, 
            Action<CommandExceptionHandlingOptions> exceptionHandlingConfigure = null,
            Action<IReceiveEndpointConfigurator> receiveEndpointConfigure = null)
        {
            var configurator = new BusServiceConfiguration(_serviceCollection);

            configAction(configurator);

            _configs.Add(new EndpointConfigItem
            {
                QueueName = queueName,
                ReceiveEndpointConfigure = receiveEndpointConfigure,
                EndpointExceptionHandlingConfigure = exceptionHandlingConfigure,
                ServiceConfiguration = configurator
            });

            return this;
        }

        public IEnumerable<EndpointConfigItem> GetEndpointConfigs()
        {
            return _configs;
        }

        public class EndpointConfigItem
        {
            public string QueueName { get; set; }

            public Action<IReceiveEndpointConfigurator> ReceiveEndpointConfigure { get; set; }

            public Action<CommandExceptionHandlingOptions> EndpointExceptionHandlingConfigure { get; set; }

            public IBusServiceConfiguration ServiceConfiguration { get; set; }
        }
    }
}