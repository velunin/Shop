using System;
using MassInstance.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class ConsumingDependencyInjectionExtensions
    {
        public static void AddServiceHosts(
            this IServiceCollection serviceCollection, 
            Action<ICompositionServiceConfiguration> configureServices)
        {
            serviceCollection.AddSingleton<IMassInstanceConsumerFactory, DependencyInjectionMassInstanceConsumerFactory>();
            serviceCollection.AddSingleton<IMassInstanceSagaConfigurator, DependencyInjectionMassInstanceSagaConfigurator>();
            serviceCollection
                .AddSingleton<CompositionServiceConfiguratorBuilder>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var compositionServiceConfiguration =
                serviceProvider.GetRequiredService<CompositionServiceConfiguratorBuilder>();

            configureServices(compositionServiceConfiguration);

            serviceCollection.AddSingleton<IRabbitMqBusCompositionServiceConfiguratorBuilder>(compositionServiceConfiguration);
            serviceCollection.AddSingleton<ICompositionServiceConfiguration>(compositionServiceConfiguration);
        }
    }
}