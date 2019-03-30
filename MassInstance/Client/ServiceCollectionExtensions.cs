using System;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.Client
{
    public static class ServiceCollectionExtensions
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