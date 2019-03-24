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
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TService : IService;
    }

    public interface IServiceConfiguration<TService> where TService : IService
    {
        void ForQueue<TQueue>(
            Expression<Func<TService, TQueue>> queueSelector,
            Action<CommandExceptionHandlingOptions> configure = null);

        void AsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector);
    }

    public interface IQueueConfiguration<TQueue> where TQueue : IQueue
    {

    }

    public class ServiceSetConfigurator : IServiceSetConfiguration
    {
        public readonly IDictionary<string,ConfigureServiceActionBucket> _actions = new Dictionary<string,ConfigureServiceActionBucket>();

        public void AddService<TService>(
            Action<IServiceConfiguration<TService>> configureService = null, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TService : IService
        {
            throw new NotImplementedException();
        }

        public class ConfigureServiceActionBucket
        {
            public Action<object> ConfigureServiceAction { get; set; }

            public Action<object> ConfigureExceptionHandlingAction { get; set; }
        }

    }

    public class ServiceConfiguration<TService> : IServiceConfiguration<TService> where TService : IService
    {
        public void ForQueue<TQueue>(Expression<Func<TService, TQueue>> queueSelector, Action<CommandExceptionHandlingOptions> configure = null)
        {
            throw new NotImplementedException();
        }

        public void AsSaga<TQueue>(Expression<Func<TService, TQueue>> queueSelector)
        {
            throw new NotImplementedException();
        }
    }

    public class ConfigurationHelper
    {

    }
}