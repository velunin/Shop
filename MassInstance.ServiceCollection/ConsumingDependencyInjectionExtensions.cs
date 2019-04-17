using System;
using MassInstance.Configuration.ServiceMap;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class ConsumingDependencyInjectionExtensions
    {
        public static void AddMassInstance(
            this IServiceCollection serviceCollection,
            Action<ICommandTypesExtractor> configureExtractor = null)
        {
            serviceCollection.AddMassTransit();
            serviceCollection.AddSingleton<IMassInstanceConsumerFactory, DependencyInjectionMassInstanceConsumerFactory>();

            var commandTypesExtractor = new CommandTypesExtractor();
            configureExtractor?.Invoke(commandTypesExtractor);

            foreach (var commandType in commandTypesExtractor.ExtractCommands())
            {
                serviceCollection.AddScoped(CommandConsumerTypeFactory.CreateCommandConsumer(commandType));
            }

            foreach (var eventType in commandTypesExtractor.ExtractEventTypes())
            {
                serviceCollection.AddScoped(CommandConsumerTypeFactory.CreateEventConsumer(eventType));
            }

            foreach (var resultType in commandTypesExtractor.ExtractResultTypes())
            {
                serviceCollection.AddScoped(CommandConsumerTypeFactory.CreateCallbackConsumer(resultType));
            }
        }
    }
}