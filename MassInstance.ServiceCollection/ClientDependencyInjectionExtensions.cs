using System;
using MassInstance.Client;
using MassInstance.Configuration.Client;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class ClientDependencyInjectionExtensions
    {
        public static void AddServiceClient(this IServiceCollection serviceCollection, Action<IQueuesMapper> mapConfigure = null, Action<RabbitMqConfig> configureRabbitMq = null)
        {
            var map = new QueuesMapper();
            var rabbitMqConfig = new RabbitMqConfig();

            configureRabbitMq?.Invoke(rabbitMqConfig);
            mapConfigure?.Invoke(map);

            serviceCollection.AddSingleton<IQueuesMapper>(map);
            serviceCollection.AddScoped<IServiceClient, ServiceClient>();

            serviceCollection.AddSingleton(rabbitMqConfig);

        }

        public static void AddServiceClient(
            this IServiceCollection serviceCollection,
            Action<IQueuesMapperBuilder> configureBuilder,
            Action<RabbitMqConfig> configureRabbitMq)
        {
            var queuesMapperBuilder = new QueuesMapperBuilder();
            var rabbitMqConfig = new RabbitMqConfig();

            configureBuilder(queuesMapperBuilder);
            configureRabbitMq(rabbitMqConfig);

            var mapper = queuesMapperBuilder.Build();

            serviceCollection.AddSingleton(mapper);
            serviceCollection.AddSingleton(rabbitMqConfig);
            serviceCollection.AddScoped<IServiceClient, ServiceClient>();
        }
    }
}