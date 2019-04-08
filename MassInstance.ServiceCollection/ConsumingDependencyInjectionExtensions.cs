using System;
using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class ConsumingDependencyInjectionExtensions
    {
        public static void AddCommandConsumers(
            this IServiceCollection serviceCollection, 
            Action<CommandTypesExtractor> configureExtractor = null)
        {
            serviceCollection.AddSingleton<IMassInstanceConsumerFactory, DependencyInjectionMassInstanceConsumerFactory>();

            var commandTypesExtractor = new CommandTypesExtractor();

            configureExtractor?.Invoke(commandTypesExtractor);

            foreach (var commandType in commandTypesExtractor.Extract())
            {
                serviceCollection.AddScoped(CommandConsumerTypeFactory.Create(commandType));
            }
        }
    }
}