using System;
using System.Linq;
using System.Reflection;
using Automatonymous;
using MassInstance.Cqrs;
using MassInstance.Cqrs.Commands;
using MassInstance.Cqrs.Events;
using MassInstance.Cqrs.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class CqrsDependencyInjectionExtensions
    {
        public static void AddCqrs(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHandlerResolver, CqrsDependencyInjectionHandlerResolver>();
            serviceCollection.AddSingleton<ICommandProcessor, CommandProcessor>();
            serviceCollection.AddSingleton<IQueryService, QueryService>();
            serviceCollection.AddSingleton<IEventDispatcher, EventDispatcher>();
        }

        public static void AddCommandAndQueryHandlers(
            this IServiceCollection serviceCollection,
            Assembly fromAssembly,
            ServiceLifetime lifetime)
        {
            serviceCollection.AddCommandAndQueryHandlers(new[] { fromAssembly }, lifetime);
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
            serviceCollection.AddSagaStateMachines(new[] { fromAssembly }, lifetime);
        }
    }
}