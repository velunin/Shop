using System;
using MassTransit;

namespace MassInstance
{
    public interface IMassInstanceConsumerFactory
    {
        object CreateConsumer(Type consumerType);

        bool TryCreateConsumer(Type consumerType, out object consumer);

        void CreateSaga(Type sagaType, IReceiveEndpointConfigurator receiveEndpointConfigurator);
    }
}