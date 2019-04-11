using System;
using System.Threading;
using MassTransit;
using MassTransit.Configurators;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configuration;
using MassTransit.Topology;
using MassTransit.Topology.EntityNameFormatters;
using MassTransit.Topology.Topologies;

namespace MassInstance.RabbitMq
{
    public static class MassInstanceBusFactory
    {
        public static IMessageTopologyConfigurator MessageTopology => Cached.MessageTopologyValue.Value;

        public static IBusControl Create(
            Action<IMassInstanceBusFactoryConfigurator> configure, 
            IMassInstanceConsumerFactory consumerFactory)
        {
            var topologyConfiguration = new RabbitMqTopologyConfiguration(MessageTopology);
            var busConfiguration = new RabbitMqBusConfiguration(topologyConfiguration);
            var busEndpointConfiguration = busConfiguration.CreateEndpointConfiguration();

            var sagaMessageExtractor = new SagaMessageExtractor();

            var configurator = new MassInstanceBusFactoryConfigurator(
                busConfiguration, 
                busEndpointConfiguration, 
                consumerFactory, 
                sagaMessageExtractor);

            configure(configurator);

            var result = BusConfigurationResult.CompileResults(configurator.Validate());

            try
            {
                return configurator.CreateBus();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException(result, "An exception occurred during bus creation", ex);
            }
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