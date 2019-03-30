using System;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public interface ICompositionServiceConfiguration
    {
        void AddService<TService>(
            Action<IServiceConfiguration<TService>> configureService = null, 
            Action<CommandExceptionHandlingOptions> configureExceptionHandling = null) where TService : IServiceMap;
    }
}