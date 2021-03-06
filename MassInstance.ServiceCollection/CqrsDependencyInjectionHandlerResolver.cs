﻿using System;
using System.Collections.Generic;
using MassInstance.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    internal class CqrsDependencyInjectionHandlerResolver : IHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public CqrsDependencyInjectionHandlerResolver(IServiceProvider serviceProvider)
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