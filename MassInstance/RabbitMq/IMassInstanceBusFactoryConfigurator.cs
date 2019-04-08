﻿using System;
using MassInstance.Configuration;
using MassInstance.Configuration.ServiceMap;
using MassTransit.RabbitMqTransport;

namespace MassInstance.RabbitMq
{
    public interface IMassInstanceBusFactoryConfigurator : IRabbitMqBusFactoryConfigurator
    {
        IMassInstanceBusFactoryConfigurator AddService<TService>(
            IRabbitMqHost host,
            Action<IServiceConfiguration<TService>> configureService)
            where TService : IServiceMap;
    }
}