using System;
using MassTransit;

namespace MassInstance
{
    public interface IMassInstanceSagaConfigurator
    {
        void Configure(Type sagaType, IReceiveEndpointConfigurator endpointConfigurator);
    }
}