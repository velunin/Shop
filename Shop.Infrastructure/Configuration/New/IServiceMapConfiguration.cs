using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MassTransit.RabbitMqTransport;
using Shop.Services.Common;

namespace Shop.Infrastructure.Configuration.New
{
    public interface ICompositionServiceConfiguration
    {
        void AddService<TService>(
            Action<IServiceConfiguration<TService>> configureService = null, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TService : IServiceMap;
    }

    public interface IServiceConfiguration<TService> where TService : IServiceMap
    {
        void ForQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configure = null) where TQueue : IQueueMap;

        void AsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector) where TQueue : IQueueMap;
    }

    public interface IRabbitMqBusServiceConfigurator
    {
        void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }

    public interface IRabbitMqBusCompositionServiceConfigurator : IRabbitMqBusServiceConfigurator
    {
    }

    public interface IQueueConfiguration
    {
        void ForCommand<TCommand>(Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }

    public interface IRabbitMqBusQueueConfigurator
    {
        void Configure(IRabbitMqReceiveEndpointConfigurator endpointConfigurator);
    }

    public class CompositionServiceConfigurator : ICompositionServiceConfiguration, IRabbitMqBusCompositionServiceConfigurator
    {
        private readonly IDictionary<Type, IRabbitMqBusServiceConfigurator> _serviceConfigurators =
            new Dictionary<Type, IRabbitMqBusServiceConfigurator>();

        private readonly IDictionary<Type, Action<CommandExceptionHandlingOptions>> _configureExceptionHandlingActions =
            new Dictionary<Type, Action<CommandExceptionHandlingOptions>>();

        public void AddService<TService>(
            Action<IServiceConfiguration<TService>> configureService = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) 
            where TService : IServiceMap
        {
            var serviceType = typeof(TService);

            if (_serviceConfigurators.ContainsKey(serviceType))
            {
                throw new ArgumentException($"Configuration for '{serviceType}' already exist");
            }

            var serviceConfig = new ServiceConfiguration<TService>();

            configureService?.Invoke(serviceConfig);

            _serviceConfigurators.Add(serviceType, serviceConfig);
            _configureExceptionHandlingActions.Add(serviceType, configureExceptionHandling);
        }

        public void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            foreach (var serviceConfiguratorEntry in _serviceConfigurators)
            {
                var composeConfigureHandlingActions = configureExceptionHandling;
                var serviceConfigurator = serviceConfiguratorEntry.Value;

                if (_configureExceptionHandlingActions.TryGetValue(
                    serviceConfiguratorEntry.Key, 
                    out var configureExceptionHandlingForService))
                {
                    composeConfigureHandlingActions += configureExceptionHandlingForService;
                }

                serviceConfigurator.Configure(busConfigurator, composeConfigureHandlingActions);
            }
        }
    }

    public class ServiceConfiguration<TService> : IRabbitMqBusServiceConfigurator, IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly IDictionary<string, IRabbitMqBusQueueConfigurator> _queuesConfigs =
            new Dictionary<string, IRabbitMqBusQueueConfigurator>();

        public void ForQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TQueue : IQueueMap
        {
            var queueType = typeof(TQueue);

            var queueFields = queueType.GetFields(BindingFlags.Public).Where(f => f.FieldType.GetInterfaces().Any(i => i == typeof(IQueueMap)));





            throw new NotImplementedException();
        }

        public void AsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector) where TQueue : IQueueMap
        {
            throw new NotImplementedException();
        }

        public void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            throw new NotImplementedException();
        }
    }

    public class QueueConfiguration : IQueueConfiguration, IRabbitMqBusQueueConfigurator
    {
        public void ForCommand<TCommand>(Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            throw new NotImplementedException();
        }

        public void Configure(IRabbitMqReceiveEndpointConfigurator endpointConfigurator)
        {
            throw new NotImplementedException();
        }
    }
}