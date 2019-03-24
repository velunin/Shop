using System;
using System.Reflection;
using Automatonymous;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Cqrs;
using Shop.Cqrs.Commands;
using Shop.Cqrs.Events;
using Shop.Cqrs.Queries;
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


        public static void AddCommandAndQueryHandlers(
            this IServiceCollection serviceCollection,
            Assembly[] fromAssemblies, 
            ServiceLifetime lifetime)
        {
            serviceCollection.Scan(scan =>
                scan.FromAssemblies(fromAssemblies)
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithLifetime(lifetime));

            serviceCollection.Scan(scan =>
                scan.FromAssemblies(fromAssemblies)
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(ICommandHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithLifetime(lifetime));

            serviceCollection.Scan(scan =>
                scan.FromAssemblies(fromAssemblies)
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithLifetime(lifetime));
        }

        public static void AddCommandAndQueryHandlers(
            this IServiceCollection serviceCollection,
            Assembly fromAssembly,
            ServiceLifetime lifetime)
        {
            serviceCollection.AddCommandAndQueryHandlers(new []{fromAssembly}, lifetime);
        }

        public static void AddSagaStateMachines(
            this IServiceCollection serviceCollection,
            Assembly[] fromAssemblies,
            ServiceLifetime lifetime)
        {
            serviceCollection.Scan(scan =>
                scan.FromAssemblies(fromAssemblies)
                    .AddClasses(
                        classes => classes
                            .AssignableTo(typeof(SagaStateMachine<>)))
                    .AsImplementedInterfaces()
                    .WithLifetime(lifetime));
        }

        public static void AddSagaStateMachines(
            this IServiceCollection serviceCollection,
            Assembly fromAssembly,
            ServiceLifetime lifetime)
        {
            serviceCollection.AddSagaStateMachines(new []{fromAssembly}, lifetime);
        }

        public static void AddCqrs(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHandlerResolver, CqrsServiceCollectionHandlerResolver>();
            serviceCollection.AddSingleton<ICommandProcessor, CommandProcessor>();
            serviceCollection.AddSingleton<IQueryService, QueryService>();
            serviceCollection.AddSingleton<IEventDispatcher, EventDispatcher>();
        }
    }
}