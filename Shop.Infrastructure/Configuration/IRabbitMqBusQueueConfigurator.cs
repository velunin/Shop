using System;
using MassTransit.RabbitMqTransport;

namespace Shop.Infrastructure.Configuration
{
    public interface IRabbitMqBusQueueConfigurator
    {
        void Configure(
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }
}