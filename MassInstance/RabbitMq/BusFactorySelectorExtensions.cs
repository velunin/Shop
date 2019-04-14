using System;
using MassInstance.Bus;
using MassTransit;

namespace MassInstance.RabbitMq
{
    public static class BusFactorySelectorExtensions
    {
        public static IServiceBus CreateMassInstanceRabbitMqBus(
            this IBusFactorySelector busFactorySelector,
            IMassInstanceConsumerFactory consumerFactory,
            Action<IMassInstanceBusFactoryConfigurator> configure)
        {
            return MassInstanceBusFactory.Create(
                configure, 
                consumerFactory);
        }
    }
}
