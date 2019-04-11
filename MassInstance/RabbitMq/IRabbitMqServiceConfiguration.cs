using MassInstance.Configuration;
using MassTransit.RabbitMqTransport;

namespace MassInstance.RabbitMq
{
    public interface IRabbitMqServiceConfiguration : IServiceConfiguration
    {
        IRabbitMqHost Host { get; }
    }
}