using System;
using MassInstance.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class ConsumingDependencyInjectionExtensions
    {
        public static void AddServiceHosts(
            this IServiceCollection serviceCollection, 
            Action<ICompositionServiceConfiguration> configureServices)
        {
            serviceCollection.AddSingleton<IMassInstanceConsumerFactory, DependencyInjectionMassInstanceConsumerFactory>();
        }
    }
}