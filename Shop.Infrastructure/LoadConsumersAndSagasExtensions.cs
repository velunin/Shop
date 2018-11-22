using System;
using Automatonymous.Scoping;
using MassTransit.AutomatonymousExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Shop.Infrastructure.Configuration;
using Shop.Infrastructure.Messaging.Extensions;

namespace Shop.Infrastructure
{
    public static class LoadConsumersAndSagasExtensions
    {
        public static void LoadServices(this IRabbitMqBusFactoryConfigurator busFactoryConfigurator,
            IServiceProvider provider, IRabbitMqHost host)
        {
            var config =
                (IBusServiceEndpointsConfiguration) provider.GetRequiredService(
                    typeof(IBusServiceEndpointsConfiguration));

            foreach (var endpointConfig in config.GetEndpointConfigs())
            {
                busFactoryConfigurator.ReceiveEndpoint(host, endpointConfig.QueueName, e =>
                {
                    e.UseCommandExceptionHandling(endpointConfig.EndpointExceptionHandlingConfigure);

                    endpointConfig.ReceiveEndpointConfigure?.Invoke(e);

                    foreach (var commandConfigItem in endpointConfig.ServiceConfiguration.GetCommandConfigs())
                    {
                        e.CommandConsumer(commandConfigItem.MessageType, provider, commandConfigItem.ExceptionHandlingConfigure);
                    }

                    foreach (var eventType in endpointConfig.ServiceConfiguration.GetEventsTypes())
                    {
                        e.EventConsumer(eventType, provider);
                    }

                    var stateMachineFactory = new DependencyInjectionSagaStateMachineFactory(provider);
                    var repositoryFactory = new DependencyInjectionStateMachineSagaRepositoryFactory(provider);
                    foreach (var sagaType in endpointConfig.ServiceConfiguration.GetSagasTypes())
                    {
                        StateMachineSagaConfiguratorCache.Configure(
                            sagaType, 
                            e,
                            stateMachineFactory,
                            repositoryFactory);
                    }
                });
            }
        }
    }
}