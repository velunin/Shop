using System;
using MassTransit.RabbitMqTransport;

namespace MassInstance.Configuration
{
    public interface IRabbitMqBusServiceConfigurator
    {
        void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator,
            IRabbitMqHost host,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }
}