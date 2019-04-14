using System;
using MassInstance.Client;
using MassInstance.Configuration.Client;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class ClientDependencyInjectionExtensions
    {
        public static void AddServiceClient(
            this IServiceCollection serviceCollection,
            Action<IQueuesMapperBuilder> configureBuilder,
            Action<SerivceClientConfig> configureRabbitMq)
        {
            var queuesMapperBuilder = new QueuesMapperBuilder();
            var rabbitMqConfig = new SerivceClientConfig();

            configureBuilder(queuesMapperBuilder);
            configureRabbitMq(rabbitMqConfig);

            var mapper = queuesMapperBuilder.Build();

            serviceCollection.AddSingleton(mapper);
            serviceCollection.AddSingleton(rabbitMqConfig);
            serviceCollection.AddScoped<IServiceClient, ServiceClient>();
        }
    }
}