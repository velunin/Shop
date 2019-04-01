using System;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    internal class DependencyInjectionCommandConsumerFactory : ICommandConsumerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyInjectionCommandConsumerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object CreateConsumer(Type consumerType)
        {
            return _serviceProvider.GetRequiredService(consumerType);
        }
    }
}