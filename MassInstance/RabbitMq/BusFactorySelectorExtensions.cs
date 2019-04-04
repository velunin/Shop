using System;
using System.Threading;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.RabbitMqTransport.Configurators;
using MassTransit.Topology;
using MassTransit.Topology.Topologies;
using MassTransit.Topology.EntityNameFormatters;

namespace MassInstance.RabbitMq
{
    public static class BusFactorySelectorExtensions
    {
        public static IBusControl CreateMassInstanceRabbitMqBus(this IBusFactorySelector busFactorySelector, Action<IMassInstanceBusFactoryConfigurator> configure)
        {
            return MassInstanceBusFactory.Create(configure);
        }
    }

    public interface IMassInstanceBusFactoryConfigurator : IRabbitMqBusFactoryConfigurator
    {
    }

    public class MassInstanceBusFactoryConfigurator : RabbitMqBusFactoryConfigurator, IMassInstanceBusFactoryConfigurator
    {
        public MassInstanceBusFactoryConfigurator(
            IRabbitMqBusConfiguration configuration, 
            IRabbitMqEndpointConfiguration busEndpointConfiguration) 
            : base(configuration, busEndpointConfiguration)
        {
        }
    }

    public static class MassInstanceBusFactory
    {
        public static IMessageTopologyConfigurator MessageTopology => Cached.MessageTopologyValue.Value;

        public static IBusControl Create(Action<IMassInstanceBusFactoryConfigurator> configure)
        {
            var topologyConfiguration = new RabbitMqTopologyConfiguration(MessageTopology);
            var busConfiguration = new RabbitMqBusConfiguration(topologyConfiguration);
            var busEndpointConfiguration = busConfiguration.CreateEndpointConfiguration();

            var configurator = new MassInstanceBusFactoryConfigurator(busConfiguration, busEndpointConfiguration);

            configure(configurator);

            return configurator.Build();
        }


        private static class Cached
        {
            internal static readonly Lazy<IMessageTopologyConfigurator> MessageTopologyValue =
                new Lazy<IMessageTopologyConfigurator>(() => new MessageTopology(EntityNameFormatter),
                    LazyThreadSafetyMode.PublicationOnly);

            private static readonly IEntityNameFormatter EntityNameFormatter;

            static Cached()
            {
                EntityNameFormatter = new MessageNameFormatterEntityNameFormatter(new RabbitMqMessageNameFormatter());
            }
        }
    }
}
