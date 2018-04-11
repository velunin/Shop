using Microsoft.Extensions.DependencyInjection;

namespace Rds.CaraBus.RequestResponse.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCaraBusRequestClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ICorrelationValueResolver, CorrelationValueResolver>();
            serviceCollection.AddTransient(typeof(IRequestClient<,>), typeof(RequestClient<,>));

            return serviceCollection;
        }
    }
}