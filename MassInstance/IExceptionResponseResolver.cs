using System;
using MassTransit;

namespace MassInstance
{
    public interface IExceptionResponseResolver
    {
        void Map(Type commandType, CommandExceptionHandlingOptions options);

        bool TryResolveResponse(Type commandType, Exception ex, out ExceptionResponse response);
    }

    public interface ISagaConfigurator
    {
        void Configure(Type sagaType, IReceiveEndpointConfigurator endpointConfigurator);
    }
}