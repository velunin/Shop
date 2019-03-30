﻿using System;
using System.Collections.Generic;
using MassTransit;

namespace MassInstance.Configuration.Old
{
    public interface IBusServiceEndpointsConfiguration
    {
        IBusServiceEndpointsConfiguration AddServiceEndpoint(
            string queueName,
            Action<IBusServiceConfiguration> config,
            Action<CommandExceptionHandlingOptions> exceptionHandlingConfigure = null,
            Action<IReceiveEndpointConfigurator> receiveEndpointConfigure = null);

        IEnumerable<BusServiceEndpointsConfiguration.EndpointConfigItem> GetEndpointConfigs();
    }
}