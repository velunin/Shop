using System;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace Shop.Infrastructure.Configuration
{
    public interface IBusServiceEndpointsConfigurator
    {
        IBusServiceEndpointsConfigurator AddServiceEndpoint(
            string queueName,
            Action<IBusServiceConfigurator> config,
            Action<CommandExceptionHandlingOptions> exceptionHandlingConfig = null,
            Action<IReceiveEndpointConfigurator> receiveEndpointConfigure = null);

        void Configure(IRabbitMqBusFactoryConfigurator busFactoryConfigurator, IServiceProvider provider, IRabbitMqHost host);
    }
}