using System;
using MassTransit;

namespace MassInstance.RabbitMq
{
    public static class BusFactorySelectorExtensions
    {
        public static IBusControl CreateMassInstanceRabbitMqBus(
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
