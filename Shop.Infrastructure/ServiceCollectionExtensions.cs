using System;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Rds.Cqrs.Commands;
using Shop.Infrastructure.Configuration;
using Shop.Infrastructure.Messaging;
using Shop.Services.Common.MessageContracts;

namespace Shop.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommandConsumer<TCommand>(this IServiceCollectionConfigurator configurator) where TCommand : class, ICommand
        {
            configurator.AddConsumer<CommandRequestConsumer<TCommand,EmptyResult>>();
        }

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