using System;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Shop.Infrastructure.Configuration;

namespace Shop.Infrastructure
{
    public static class BusFactoryConfigurationExtensions
    {
        public static void ConsumeServices(this IRabbitMqBusFactoryConfigurator busFactoryConfigurator,
            IServiceProvider provider, IRabbitMqHost host)
        {
            var config =
                (IBusServiceEndpointsConfigurator) provider.GetRequiredService(
                    typeof(IBusServiceEndpointsConfigurator));

            config.Configure(busFactoryConfigurator, provider, host);
        }
    }
}