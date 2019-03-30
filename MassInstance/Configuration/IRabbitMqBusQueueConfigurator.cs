using System;
using MassTransit.RabbitMqTransport;

namespace MassInstance.Configuration
{
    public interface IRabbitMqBusQueueConfigurator
    {
        void Configure(
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }
}