using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSaga;
using RDS.CaraBus;

namespace Shop.Infrastructure.SubscriptionConfigurators
{
    internal class SagasSubscriptionConfigurator : ISubscriptionConfigurator
    {
        private readonly ICaraBus _caraBus;
        private readonly ISagaMediator _mediator;

        public SagasSubscriptionConfigurator(ICaraBus caraBus, ISagaMediator mediator)
        {
            _caraBus = caraBus;
            _mediator = mediator;
        }

        public async Task ConfigureAsync(IEnumerable<Type> sourceTypes, CancellationToken cancellationToken)
        {
            var sagaInterfaces = new[]
            {
                typeof(InitiatedBy<>),
                typeof(ConsumerOf<>)
            };

            foreach (var sagaType in sourceTypes)
            {
                var interfaceTypes = sagaType
                    .GetInterfaces()
                    .Where(x => x.IsGenericType && sagaInterfaces.Contains(x.GetGenericTypeDefinition()));

                foreach (var interfaceType in interfaceTypes)
                {
                    var messageType = interfaceType.GenericTypeArguments.First();
                    var faultMessageCreator = ReflectionHelper.GetFaultMessageCreator(messageType);

                    await _caraBus.SubscribeAsync(messageType,
                        async (message,token) => await OnRevievedSagaMessage(messageType, message, faultMessageCreator, token),
                        SubscribeOptions.Exclusive($"{sagaType.FullName}_{messageType.FullName}"), cancellationToken);
                }
            }
        }

        private async Task OnRevievedSagaMessage(Type messageType, object message, Func<object, Exception, object> faultMessageCreator, CancellationToken cancellationToken)
        {
            try
            {
                try
                {
                    if (IsInitMessage(messageType))
                    {
                        _mediator.Consume(message as IInitiatingSagaMessage);
                    }
                    else
                    {
                        _mediator.Consume(message as ISagaMessage);
                    }
                }
                catch (TargetInvocationException tie)
                {
                    throw tie.InnerException;
                }
            }
            catch (Exception ex)
            {
                await _caraBus.PublishAsync(
                    faultMessageCreator(message, ex), 
                    cancellationToken: cancellationToken);
            }
        }

        private static bool IsInitMessage(Type commandType)
        {
            return typeof(IInitiatingSagaMessage).IsAssignableFrom(commandType);
        }
    }
}