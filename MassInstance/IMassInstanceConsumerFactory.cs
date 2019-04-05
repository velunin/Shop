using System;
using MassTransit;

namespace MassInstance
{
    public interface IMassInstanceConsumerFactory
    {
        object CreateConsumer(Type commandType);

        void CreateSaga(Type sagaType, IReceiveEndpointConfigurator receiveEndpointConfigurator);
    }
}