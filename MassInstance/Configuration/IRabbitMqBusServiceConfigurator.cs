using System;

namespace MassInstance.Configuration
{
    public interface IRabbitMqBusServiceConfigurator
    {
        void Build(Action<CommandExceptionHandlingOptions> configureExceptionHandling = null);
    }
}