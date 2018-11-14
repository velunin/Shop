using Microsoft.Extensions.DependencyInjection;

namespace Shop.Services.Common
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServiceClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IServiceClient, ServiceClient>();
        }
    }
} 