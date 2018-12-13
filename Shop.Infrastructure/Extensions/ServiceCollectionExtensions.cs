using System;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Infrastructure.Configuration;

namespace Shop.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
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
    }
}