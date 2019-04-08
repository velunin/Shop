using System;
using MassTransit;

namespace MassInstance
{
    public interface IMassInstanceConsumerFactory
    {
        object CreateConsumer(Type consumerType);

        void CreateSaga(Type sagaType, IReceiveEndpointConfigurator receiveEndpointConfigurator);
    }
}