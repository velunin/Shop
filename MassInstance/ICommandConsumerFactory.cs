using System;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance
{
    public interface ICommandConsumerFactory
    {
        object CreateConsumer(Type commandType);

    }

    public class ServiceCollectionCommandConsumerFactory : ICommandConsumerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceCollectionCommandConsumerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object CreateConsumer(Type consumerType)
        {
            return _serviceProvider.GetRequiredService(consumerType);
        }
    }
}