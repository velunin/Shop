﻿using System;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;
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
            Action<IBusServiceEndpointsConfigurator> configureServiceEndpoints = null, 
            Action<IServiceCollectionConfigurator> configureServiceCollection = null)
        {
            serviceCollection.AddMassTransit(configureServiceCollection);

            var busServiceEndpointsConfigurator = new BusServiceEndpointsConfigurator(serviceCollection);

            configureServiceEndpoints?.Invoke(busServiceEndpointsConfigurator);

            serviceCollection.AddSingleton<IBusServiceEndpointsConfigurator>(busServiceEndpointsConfigurator);

            serviceCollection.AddSingleton<EventDispatcher>();
            serviceCollection.AddSingleton(factory =>
            {
                IEventDispatcher UnderlyingDispatcherAccessor() => factory.GetService<EventDispatcher>();
                return (Func<IEventDispatcher>)UnderlyingDispatcherAccessor;
            });
            serviceCollection.Decorate<IEventDispatcher>((inner, provider) =>
                new MasstransitEventDispatcher(provider.GetRequiredService<IBus>()));
        }
    }
}