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
            serviceCollection.AddSingleton<IExceptionResponseResolver, ExceptionResponseResolver>();
            serviceCollection.AddSingleton<ICommandConsumerFactory, DependencyInjectionCommandConsumerFactory>();
            serviceCollection.AddSingleton<ISagaConfigurator, DependencyInjectionSagaConfigurator>();
            serviceCollection
                .AddSingleton<CompositionServiceConfigurator>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var compositionServiceConfiguration =
                serviceProvider.GetRequiredService<CompositionServiceConfigurator>();

            configureServices(compositionServiceConfiguration);

            serviceCollection.AddSingleton<IRabbitMqBusCompositionServiceConfigurator>(compositionServiceConfiguration);
            serviceCollection.AddSingleton<ICompositionServiceConfiguration>(compositionServiceConfiguration);
        }
    }
}