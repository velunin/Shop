using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using NSaga;

using Rds.CaraBus.RequestResponse;
using Rds.Cqrs.Commands;

using Shop.Domain.Commands;
using Shop.Infrastructure.SubscriptionConfigurators;

namespace Shop.Infrastructure
{
    public abstract class AppServiceBusBootstrapBase : IAppServiceBusBootstrap
    {
        private readonly ICorrelationValueResolver _correlationValueResolver;
        private readonly Func<SubscribersType, ISubscriptionConfigurator> _subscriptionConfiguratorFactory;

        protected AppServiceBusBootstrapBase(
            ICorrelationValueResolver correlationValueResolver,
            Func<SubscribersType, ISubscriptionConfigurator> subscriptionConfiguratorFactory)
        {
            _correlationValueResolver = correlationValueResolver;
            _subscriptionConfiguratorFactory = subscriptionConfiguratorFactory;
        }

        protected abstract IEnumerable<Type> AllowedCommands { get; }

        protected abstract IEnumerable<Type> AllowedSagas { get; }

        protected abstract IEnumerable<Type> AllowedEvents { get; }

        public void Startup(CancellationToken cancellationToken)
        {
            Task.WhenAll(GetSubscriptionConfigurators(cancellationToken))
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            ReqisterCorrelationValuesGetters();
        }

        private IEnumerable<Task> GetSubscriptionConfigurators(CancellationToken cancellationToken)
        {
            yield return _subscriptionConfiguratorFactory(SubscribersType.Sagas)
                .ConfigureAsync(AllowedSagas, cancellationToken);

            yield return _subscriptionConfiguratorFactory(SubscribersType.Commands)
                .ConfigureAsync(AllowedCommands, cancellationToken);

            yield return _subscriptionConfiguratorFactory(SubscribersType.Events)
                .ConfigureAsync(AllowedEvents, cancellationToken);
        }

        private void ReqisterCorrelationValuesGetters()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var commandsWithCorrelation = GetCommandsWithCorrelation(assemblies);
            var sagaMessages = GetSagaMessages(assemblies);
            var eventsWithCorrelation = GetEventsWithCorrelation(assemblies);
           
            foreach (var commandWithCorrelation in commandsWithCorrelation)
            {
                var commandExecutedType = typeof(CommandExecuted<>).MakeGenericType(commandWithCorrelation);

                _correlationValueResolver.UseCorrelationId(commandWithCorrelation,
                    o => ((ICorrelatedCommand) o).CorrelationId);

                _correlationValueResolver.UseCorrelationId(commandExecutedType,
                    GetCorrelationValueFromCommandExecutedEvent);
            }

            foreach (var sagaMessage in sagaMessages)
            {
                _correlationValueResolver.UseCorrelationId(sagaMessage, o => ((ISagaMessage)o).CorrelationId);
            }

            foreach (var eventWithCorrelation in eventsWithCorrelation)
            {
                _correlationValueResolver.UseCorrelationId(eventWithCorrelation, o => ((ICorrelatedEvent)o).CorrelationId);
            }
        }

        private static Guid GetCorrelationValueFromCommandExecutedEvent(object o)
        {
            var p = Expression.Parameter(typeof(object));
            var expr = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.PropertyOrField(
                        Expression.Convert(p, o.GetType()), "Command"), typeof(object)), p);

            return ((ICorrelatedCommand) expr.Compile().Invoke(o)).CorrelationId;
        }

        private static IEnumerable<Type> GetEventsWithCorrelation(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(
                x => x
                    .GetTypes()
                    .Where(
                        t => t.IsClass && t.GetInterfaces().Any(i => i == typeof(ICorrelatedEvent))));
        }

        private static IEnumerable<Type> GetSagaMessages(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(
                x => x
                    .GetTypes()
                    .Where(
                        t => t.IsClass && t.GetInterfaces().Any(i => i == typeof(ISagaMessage))));
        }

        private static IEnumerable<Type> GetCommandsWithCorrelation(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(
                x => x
                    .GetTypes()
                    .Where(
                        t => t.IsClass && t.GetInterfaces().Any(i => i == typeof(ICorrelatedCommand))));
        }
    }
}