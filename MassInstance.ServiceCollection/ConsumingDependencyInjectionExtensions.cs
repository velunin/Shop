using System;
using MassInstance.Configuration;
using MassInstance.Configuration.Old;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class ConsumingDependencyInjectionExtensions
    {
        public static void AddServices(
            this IServiceCollection serviceCollection, 
            Action<IBusServiceEndpointsConfiguration> configureServiceEndpoints = null, 
            Action<IServiceCollectionConfigurator> configureServiceCollection = null)
        {
            serviceCollection.AddMassTransit(configureServiceCollection);

            var busServiceEndpointsConfigurator = new BusServiceEndpointsConfiguration(serviceCollection);

            configureServiceEndpoints?.Invoke(busServiceEndpointsConfigurator);

            serviceCollection.AddSingleton<IBusServiceEndpointsConfiguration>(busServiceEndpointsConfigurator);
        }

        public static void AddServiceHosts(
            this IServiceCollection serviceCollection, 
            Action<ICompositionServiceConfiguration> configureServices)
        {
            serviceCollection.AddSingleton<ICommandConsumerFactory, DependencyInjectionCommandConsumerFactory>();
            serviceCollection.AddSingleton<IExceptionResponseResolver, ExceptionResponseResolver>();
            serviceCollection.AddSingleton<ISagaConfigurator, DependencyInjectionSagaConfigurator>();
            serviceCollection
                .AddSingleton<IRabbitMqBusCompositionServiceConfigurator, CompositionServiceConfigurator>();

            serviceCollection.AddSingleton<ICompositionServiceConfiguration>(provider =>
                provider.GetRequiredService<IRabbitMqBusCompositionServiceConfigurator>());

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var compositionServiceConfiguration =
                serviceProvider.GetRequiredService<ICompositionServiceConfiguration>();

            configureServices(compositionServiceConfiguration);
        }
    }
}