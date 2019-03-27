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
        void Configure(
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }

    public class CompositionServiceConfigurator : ICompositionServiceConfiguration, IRabbitMqBusCompositionServiceConfigurator
    {
        private readonly IDictionary<Type, ServiceConfiguratorItem> _serviceConfigurators =
            new Dictionary<Type, ServiceConfiguratorItem>();

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

            _serviceConfigurators.Add(
                serviceType, 
                new ServiceConfiguratorItem
                {
                    Configurator = serviceConfig,
                    ConfigureExceptionHandling = configureExceptionHandling
                });
        }

        public void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            foreach (var serviceConfiguratorEntry in _serviceConfigurators)
            {
                var serviceConfiguratorItem = serviceConfiguratorEntry.Value;
                
                serviceConfiguratorItem
                    .Configurator
                    .Configure(
                        busConfigurator, 
                        configureExceptionHandling + serviceConfiguratorItem.ConfigureExceptionHandling);
            }
        }

        public class ServiceConfiguratorItem
        {
            public IRabbitMqBusServiceConfigurator Configurator { get; set; }

            public Action<CommandExceptionHandlingOptions> ConfigureExceptionHandling { get; set; }
        }
    }

    public class ServiceConfiguration<TService> : IRabbitMqBusServiceConfigurator, IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly IDictionary<string, QueueConfiguratorItem> _queuesConfigsOverrides =
            new Dictionary<string, QueueConfiguratorItem>();

        private readonly IDictionary<string, Action<CommandExceptionHandlingOptions>> _configureExceptionHandlingActions =
            new Dictionary<string, Action<CommandExceptionHandlingOptions>>();

        public void ForQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TQueue : IQueueMap
        {
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
            var serviceType = typeof(TService);
            var queueFields = serviceType
                .GetFields(BindingFlags.Public)
                .Where(f => f
                        .FieldType
                        .GetInterfaces()
                        .Any(i => i == typeof(IQueueMap)));

            foreach (var queueField in queueFields)
            {
                var composeConfigureHandlingActions = configureExceptionHandling;
                if (_queuesConfigsOverrides.TryGetValue(
                    queueField.Name, 
                    out var configuratorItem))
                {
                    configuratorItem.Configurator.Configure();
                    composeConfigureHandlingActions += configureExceptionHandlingForQueue;
                }
            }

            throw new NotImplementedException();
        }

        public class QueueConfiguratorItem
        {
            public IRabbitMqBusQueueConfigurator Configurator { get; set; }

            public Action<CommandExceptionHandlingOptions> ConfigureExceptionHandling { get; set; }
        }
    }

    public class QueueConfiguration : IQueueConfiguration, IRabbitMqBusQueueConfigurator
    {
        public void ForCommand<TCommand>(Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            throw new NotImplementedException();
        }

        public void Configure(IRabbitMqReceiveEndpointConfigurator endpointConfigurator, Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            throw new NotImplementedException();
        }
    }
}