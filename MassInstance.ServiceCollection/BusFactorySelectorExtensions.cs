using System;
using MassInstance.RabbitMq;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace MassInstance.ServiceCollection
{
    public static class BusFactorySelectorExtensions
    {
        public static IBusControl CreateMassInstanceRabbitMqBus(
            this IBusFactorySelector busFactorySelector,
            IServiceProvider serviceProvider,
            Action<IMassInstanceBusFactoryConfigurator> configure)
        {
            var commandConsumerFactory = serviceProvider.GetRequiredService<IMassInstanceConsumerFactory>();

            return MassInstanceBusFactory.Create(configure, commandConsumerFactory);
        }
    }
}