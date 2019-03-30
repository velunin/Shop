using System;
using MassTransit.RabbitMqTransport;

namespace Shop.Infrastructure.Configuration
{
    public interface IRabbitMqBusServiceConfigurator
    {
        void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator,
            IRabbitMqHost host,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }
}