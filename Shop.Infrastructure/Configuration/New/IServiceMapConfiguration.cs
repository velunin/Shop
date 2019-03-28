﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using MassTransit.RabbitMqTransport;
using Shop.Cqrs.Commands;
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
            Action<IQueueConfiguration<TQueue>> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configure = null) where TQueue : IQueueMap;

        void AsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector) where TQueue : IQueueMap;
    }

    public interface IRabbitMqBusServiceConfigurator
    {
        void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator,
            IRabbitMqHost host,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }

    public interface IRabbitMqBusCompositionServiceConfigurator : IRabbitMqBusServiceConfigurator
    {
    }

    public interface IQueueConfiguration<TQueue> where TQueue : IQueueMap
    {
        void ForCommand<TCommand>(Expression<Func<TQueue, TCommand>> commandSelector, Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }

    public interface IRabbitMqBusQueueConfigurator
    {
        void Configure(
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }

    public class CompositionServiceConfigurator : ICompositionServiceConfiguration, IRabbitMqBusCompositionServiceConfigurator
    {
        private readonly IDictionary<Type, (IRabbitMqBusServiceConfigurator, Action<CommandExceptionHandlingOptions>)> _serviceConfigurators =
            new Dictionary<Type, (IRabbitMqBusServiceConfigurator, Action<CommandExceptionHandlingOptions>)>();

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
                (serviceConfig, configureExceptionHandling));
        }

        public void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator,
            IRabbitMqHost host,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            foreach (var serviceConfiguratorEntry in _serviceConfigurators)
            {
                var (serviceConfiguratorItem, configureExceptionHanlingForService) = serviceConfiguratorEntry.Value;
                
                serviceConfiguratorItem
                    .Configure(
                        busConfigurator,
                        host,
                        configureExceptionHandling + configureExceptionHanlingForService);
            }
        }
    }

    public class ServiceConfiguration<TService> : IRabbitMqBusServiceConfigurator, IServiceConfiguration<TService> where TService : IServiceMap
    {
        private readonly IDictionary<string, (QueueConfiguratorBase, Action<CommandExceptionHandlingOptions>)> _queuesConfigsOverrides =
            new Dictionary<string, (QueueConfiguratorBase, Action<CommandExceptionHandlingOptions>)>();

        public void ForQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration<TQueue>> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TQueue : IQueueMap
        {
            var queueConfiguration = new QueueConfiguration<TQueue>();
            var queueName = ((MemberExpression) queueSelector.Body).Member.Name;

            configureQueue?.Invoke(queueConfiguration);

            _queuesConfigsOverrides.Add(queueName, (queueConfiguration,configureExceptionHandling));
        }

        public void AsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector) where TQueue : IQueueMap
        {
            throw new NotImplementedException();
        }

        public void Configure(
            IRabbitMqBusFactoryConfigurator busConfigurator, 
            IRabbitMqHost host,
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
                var queueConfigurator = new QueueConfiguratorBase(queueField.FieldType);
                var composeConfigureHandlingActions = configureExceptionHandling;

                if (_queuesConfigsOverrides.TryGetValue(
                    queueField.Name, 
                    out var queueConfigOverrideEntry))
                {
                    var (queueConfiguratorOverride, commandExceptionHandlingForQueue) = queueConfigOverrideEntry;
                    queueConfigurator = queueConfiguratorOverride;

                    composeConfigureHandlingActions += commandExceptionHandlingForQueue;
                }
                
                busConfigurator.ReceiveEndpoint(host, ExtractQueueName(queueField), e =>
                {
                    queueConfigurator.Configure(e, composeConfigureHandlingActions);
                });
            }
        }

        private static string ExtractQueueName(MemberInfo fieldInfo)
        {
            return Regex.Replace(
                    fieldInfo.Name,
                    "([A-Z])", "-$0",
                    RegexOptions.Compiled)
                .Trim('-')
                .ToLower();
        }
    }

    public class QueueConfiguration<TQueue> : QueueConfiguratorBase, IQueueConfiguration<TQueue> where TQueue : IQueueMap
    {
        public void ForCommand<TCommand>(Expression<Func<TQueue,TCommand>> commandSelector, Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            throw new NotImplementedException();
        }

        public QueueConfiguration() : base(typeof(TQueue))
        {
        }
    }

    public class QueueConfiguratorBase : IRabbitMqBusQueueConfigurator
    {
        private readonly Type _queueType;

        public QueueConfiguratorBase(Type queueType)
        {
            _queueType = queueType;
        }

        public void Configure(
            IRabbitMqReceiveEndpointConfigurator endpointConfigurator, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null)
        {
            var commandFields = _queueType.GetFields(BindingFlags.Public)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(ICommand)));

            foreach (var commandField in commandFields)
            {
                
            }
            throw new NotImplementedException();
        }
    }
}