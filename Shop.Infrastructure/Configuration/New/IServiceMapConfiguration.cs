using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Shop.Services.Common;

namespace Shop.Infrastructure.Configuration.New
{
    public interface IServiceSetConfiguration
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

    public interface IQueueConfiguration
    {
        void ForCommand<TCommand>(Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }

    public class ServiceSetConfigurator : IServiceSetConfiguration
    {
        private readonly IDictionary<Type, ConfigureServiceActionBucket> _configureActionsForServices = new Dictionary<Type, ConfigureServiceActionBucket>();

        public void AddService<TService>(
            Action<IServiceConfiguration<TService>> configureService = null, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TService : IServiceMap
        {
            var serviceType = typeof(TService);

            if (_configureActionsForServices.ContainsKey(serviceType))
            {
                throw new ArgumentException($"Configuration for '{serviceType}' already exist");
            }

            var serviceConfig = new ServiceConfiguration<TService>();

            if (!_configureActionsForServices.ContainsKey(serviceType))
            {
                _configureActionsForServices.Add(serviceType, new ConfigureServiceActionBucket
                {
                    ConfigureServiceAction = configureService != null
                        ? new Action<object>(p => configureService((IServiceConfiguration<TService>) p))
                        : null,

                    ConfigureExceptionHandlingAction = configureExceptionHandling
                });
            }

            throw new NotImplementedException();
        }

        public class ConfigureServiceActionBucket
        {
            public Action<object> ConfigureServiceAction { get; set; }

            public Action<CommandExceptionHandlingOptions> ConfigureExceptionHandlingAction { get; set; }
        }
    }

    public class ServiceConfiguration<TService> : IServiceConfiguration<TService> where TService : IServiceMap
    {
        public void ForQueue<TQueue>(Expression<Func<TService, TQueue>> queueSelector,
            Action<IQueueConfiguration> configureQueue = null,
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TQueue : IQueueMap
        {
            throw new NotImplementedException();
        }

        public void AsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector) where TQueue : IQueueMap
        {
            throw new NotImplementedException();
        }
    }
}