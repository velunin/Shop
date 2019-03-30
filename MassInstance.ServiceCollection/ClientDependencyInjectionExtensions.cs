using System;
using MassInstance.Client;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class ClientDependencyInjectionExtensions
    {
        public static void AddServiceClient(this IServiceCollection serviceCollection, Action<IQueuesMapper> mapConfigure = null)
        {
            var map = new QueuesMapper();

            mapConfigure?.Invoke(map);

            serviceCollection.AddSingleton<IQueuesMapper>(map);
            serviceCollection.AddScoped<IServiceClient, ServiceClient>();
        }
    }
}