using System;
using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;
using MassTransit.RabbitMqTransport;

namespace MassInstance.RabbitMq
{
    public class RabbitMqServiceConfiguration<TService> : ServiceConfiguration<TService>, 
        IRabbitMqServiceConfiguration 
        where TService : IServiceMap
    {
        public RabbitMqServiceConfiguration(IRabbitMqHost host)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public IRabbitMqHost Host { get; }
    }
}