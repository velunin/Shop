﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Automatonymous.Scoping;
using MassTransit;
using MassTransit.AutomatonymousExtensionsDependencyInjectionIntegration;


namespace MassInstance.ServiceCollection
{
    internal class DependencyInjectionMassInstanceConsumerFactory : IMassInstanceConsumerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyInjectionMassInstanceConsumerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object CreateConsumer(Type consumerType)
        {
            return _serviceProvider.GetRequiredService(consumerType);
        }

        public void CreateSaga(Type sagaType, IReceiveEndpointConfigurator receiveEndpointConfigurator)
        {
            var stateMachineFactory = new DependencyInjectionSagaStateMachineFactory(_serviceProvider);
            var repositoryFactory = new DependencyInjectionStateMachineSagaRepositoryFactory(_serviceProvider);

            StateMachineSagaConfiguratorCache.Configure(
                sagaType,
                receiveEndpointConfigurator,
                stateMachineFactory,
                repositoryFactory);
        }
    }
}