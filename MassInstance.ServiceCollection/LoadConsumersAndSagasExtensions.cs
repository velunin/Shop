using System;
using MassInstance.Configuration;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class LoadConsumersAndSagasExtensions
    {
        public static void LoadServices(
            this IRabbitMqBusFactoryConfigurator busFactoryConfigurator,
            IServiceProvider provider, 
            IRabbitMqHost host)
        {
            var compositionServiceCfg = provider.GetRequiredService<IRabbitMqBusCompositionServiceConfiguratorBuilder>();

            compositionServiceCfg.Build(
                busFactoryConfigurator, 
                host);
        }
    }
}