using System;
using MassTransit;

namespace MassInstance.RabbitMq
{
    public static class BusFactorySelectorExtensions
    {
        public static IBusControl CreateMassInstanceRabbitMqBus(this IBusFactorySelector busFactorySelector, Action<IMassInstanceBusFactoryConfigurator> configure)
        {
            throw new NotImplementedException();
        }
    }

    public interface IMassInstanceBusFactoryConfigurator
    {

    }
}
