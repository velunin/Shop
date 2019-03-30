using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Shop.Cqrs;

namespace MassInstance.ServiceCollection
{
    internal class CqrsServiceCollectionHandlerResolver : IHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public CqrsServiceCollectionHandlerResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object ResolveHandler(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        public IEnumerable<object> ResolveHandlers(Type type)
        {
            return _serviceProvider.GetServices(type);
        }
    }
}