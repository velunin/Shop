using MassInstance.Client;
using MassTransit;

namespace MassInstance.Bus
{
    public interface IServiceBus : IBusControl, IServiceClient
    {
    }
}
