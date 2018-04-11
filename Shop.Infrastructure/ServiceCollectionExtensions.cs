using System;

using Microsoft.Extensions.DependencyInjection;

using Shop.Infrastructure.SubscriptionConfigurators;

namespace Shop.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppInfrastructure<TServiceBusBootstrap>(this IServiceCollection services)
            where TServiceBusBootstrap : AppServiceBusBootstrapBase
        {
            services.AddSingleton<SagasSubscriptionConfigurator>();
            services.AddSingleton<CommandsSubscriptionConfigurator>();
            services.AddSingleton<EventsSubscriptionConfigurator>();

            services.AddSingleton<Func<SubscribersType,ISubscriptionConfigurator>>(serviceProvider =>
            {
                return subscribersType =>
                {
                    switch (subscribersType)
                    {
                        case SubscribersType.Sagas:
                            return serviceProvider.GetService<SagasSubscriptionConfigurator>();
                        case SubscribersType.Commands:
                            return serviceProvider.GetService<CommandsSubscriptionConfigurator>();
                        case SubscribersType.Events:
                            return serviceProvider.GetService<EventsSubscriptionConfigurator>();
                        default: throw new NotSupportedException($@"Subscribers type ""{subscribersType}"" not supported");
                    }
                };
            });

            services.AddSingleton<IAppServiceBusBootstrap, TServiceBusBootstrap>();

            return services;
        }
    }
}