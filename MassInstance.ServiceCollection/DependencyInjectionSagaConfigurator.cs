using System;

using Automatonymous.Scoping;

using MassTransit;
using MassTransit.AutomatonymousExtensionsDependencyInjectionIntegration;

namespace MassInstance.ServiceCollection
{
    internal class DependencyInjectionSagaConfigurator : ISagaConfigurator
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyInjectionSagaConfigurator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Configure(Type sagaType, IReceiveEndpointConfigurator endpointConfigurator)
        {
            var stateMachineFactory = new DependencyInjectionSagaStateMachineFactory(_serviceProvider);
            var repositoryFactory = new DependencyInjectionStateMachineSagaRepositoryFactory(_serviceProvider);

            StateMachineSagaConfiguratorCache.Configure(
                sagaType,
                endpointConfigurator,
                stateMachineFactory,
                repositoryFactory);
        }
    }
}