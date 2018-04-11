using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NSaga.ServiceCollection
{
    public static class ServiceCollectionNSagaIntegration

    {
        /// <summary>
        /// Registers all the saga classes and all default components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>Intance of the same container for fluent configuration.</returns>
        public static IServiceCollection AddNSagaComponents(this IServiceCollection container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            
            return container.AddNSagaComponents(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Registers all the saga classes and all default components
        /// <param>
        /// Default registrations are:
        /// <list type="bullet">
        /// <item><description><see cref="JsonNetSerialiser"/> to serialise messages; </description></item> 
        /// <item><description><see cref="InMemorySagaRepository"/> to store saga datas; </description></item> 
        /// <item><description><see cref="ServiceCollectionSagaFactory"/> to resolve instances of Sagas;</description></item> 
        /// <item><description><see cref="SagaMetadata"/> to work as the key component - SagaMediator;</description></item> 
        /// <item><description><see cref="MetadataPipelineHook"/> added to the pipeline to preserve metadata about incoming messages.</description></item> 
        /// </list>
        /// </param>
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>Intance of the same container for fluent configuration.</returns>
        public static IServiceCollection AddNSagaComponents(this IServiceCollection container, params Assembly[] assemblies)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            container.AddTransient<ISagaFactory, ServiceCollectionSagaFactory>();
            container.AddTransient<IMessageSerialiser, JsonNetSerialiser>();
            container.AddTransient<ISagaRepository, InMemorySagaRepository>();
            container.AddTransient<IPipelineHook, MetadataPipelineHook>();
            container.AddTransient<ISagaMediator, SagaMediator>();
     
            var sagaTypesDefinitions = NSagaReflection.GetAllSagasInterfaces(assemblies);
            foreach (var sagaTypesDefinition in sagaTypesDefinitions)
            {
                container.AddTransient(sagaTypesDefinition.Value, sagaTypesDefinition.Key);
            }

            var sagaTypes = NSagaReflection.GetAllSagaTypes(assemblies);
            foreach (var sagaType in sagaTypes)
            {
                container.AddTransient(sagaType);
            }

            return container;
        }

        /// <summary>
        /// Override default ISagaRepository registration with the container.
        /// </summary>
        /// <typeparam name="TSagaRepository">The type of the saga repository.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>Intance of the same container for fluent configuration.</returns>
        public static IServiceCollection UseSagaRepository<TSagaRepository>(this IServiceCollection container) where TSagaRepository : ISagaRepository
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.OverrideRegistration<ISagaRepository>(c => c.AddTransient(typeof(ISagaRepository), typeof(TSagaRepository)));

            return container;
        }


        /// <summary>
        /// Override default ISagaRepository registration with the container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <returns>Intance of the same container for fluent configuration.</returns>
        public static IServiceCollection UseSagaRepository(this IServiceCollection container, Func<ISagaRepository> repositoryFactory)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (repositoryFactory == null)
            {
                throw new ArgumentNullException(nameof(repositoryFactory));
            }

            container.OverrideRegistration<ISagaRepository>(c => c.AddTransient(factory => repositoryFactory));

            return container;
        }


        /// <summary>
        /// Adds another pipeline hook into the pipeline. <see cref="IPipelineHook"/> for description of possible interception points.
        /// </summary>
        /// <typeparam name="TPipelineHook">The type of the pipeline hook.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>Intance of the same container for fluent configuration.</returns>
        public static IServiceCollection AddSagaPipelineHook<TPipelineHook>(this IServiceCollection container) where TPipelineHook : IPipelineHook
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.AddTransient(typeof(IPipelineHook), typeof(TPipelineHook));

            return container;
        }


        /// <summary>
        /// Replaces the default implementation of <see cref="IMessageSerialiser"/>.
        /// </summary>
        /// <typeparam name="TMessageSerialiser">The type of the message serialiser.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>Intance of the same container for fluent configuration.</returns>
        public static IServiceCollection UseMessageSerialiser<TMessageSerialiser>(this IServiceCollection container) where TMessageSerialiser : IMessageSerialiser
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.OverrideRegistration<IMessageSerialiser>(c => c.AddTransient(typeof(IMessageSerialiser), typeof(TMessageSerialiser)));

            return container;
        }

        /// <summary>
        /// Replaces the default implementation of <see cref="IMessageSerialiser"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="messageSerialiserFactory">The message serialiser factory.</param>
        /// <returns>Intance of the same container for fluent configuration.</returns>
        public static IServiceCollection UseMessageSerialiser(this IServiceCollection container, Func<IMessageSerialiser> messageSerialiserFactory)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (messageSerialiserFactory == null)
            {
                throw new ArgumentNullException(nameof(messageSerialiserFactory));
            }

            container.OverrideRegistration<IMessageSerialiser>(c => c.AddTransient(factory => messageSerialiserFactory));

            return container;
        }

        private static void OverrideRegistration<T>(this IServiceCollection container, Action<IServiceCollection> act)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var serviceDescriptor = container.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
            container.Remove(serviceDescriptor);

            act.Invoke(container);
        }
    }
}
