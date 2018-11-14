
using System;
using System.Linq;
using System.Linq.Expressions;
using GreenPipes;
using MassTransit;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Events;
using Shop.Infrastructure.Messaging;
using Shop.Infrastructure.Messaging.Extensions;
using Shop.Infrastructure.Messaging.MessageContracts;

namespace Shop.Infrastructure
{
    public static class EndpointConfigurationExtensions
    {
        public static void CommandConsumer<TCommand>(this IReceiveEndpointConfigurator configuration,
            IServiceProvider provider,
            Action<CommandExceptionHandlingOptions> setupAction = null) where TCommand : ICommand
        {
            configuration.CommandConsumer(typeof(TCommand), provider, setupAction);
        }

        public static void CommandConsumer(this IReceiveEndpointConfigurator configuration,
            Type[] commandTypes,
            IServiceProvider provider,
            Action<CommandExceptionHandlingOptions> setupAction = null)
        {
            foreach (var commandType in commandTypes)
            {
                configuration.CommandConsumer(commandType, provider, setupAction);
            }
        }

        public static void CommandConsumer(this IReceiveEndpointConfigurator configuration,
            Type commandType,
            IServiceProvider provider,
            Action<CommandExceptionHandlingOptions> setupAction = null)
        {
            var commandResultTypes = commandType
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResultingCommand<>))
                .Select(i => i.GetGenericArguments().FirstOrDefault())
                .ToList();

            if (commandResultTypes.Any())
            {
                foreach (var commandResultType in commandResultTypes)
                {
                    CallGenericCommandConsumer(commandType, commandResultType, configuration, provider, setupAction);
                }
            }
            else
            {
                CallGenericCommandConsumer(commandType, typeof(EmptyResult), configuration, provider, setupAction);
            }
        }

        public static void EventConsumer<TEvent>(this IReceiveEndpointConfigurator configuration,
            IServiceProvider provider) where TEvent : class,IEvent
        {
            configuration.Consumer<EventConsumer<TEvent>>(provider);
        }

        public static void EventConsumer(this IReceiveEndpointConfigurator configuration,
            Type eventType,
            IServiceProvider provider)
        {
            var eventConsumerMethod = typeof(EndpointConfigurationExtensions)
                .GetMethods()
                .Single(m => m.Name == "EventConsumer" && m.GetGenericArguments().Length == 1)
                .MakeGenericMethod(eventType);

            var configurationParameter = Expression.Parameter(typeof(IReceiveEndpointConfigurator), "configuration");
            var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "provider");

            var body = Expression.Call(
                eventConsumerMethod,
                configurationParameter,
                serviceProviderParameter);

            var lambda =
                Expression
                    .Lambda<Action<IReceiveEndpointConfigurator, IServiceProvider>>(body, configurationParameter, serviceProviderParameter);
            var action = lambda.Compile();

            action(configuration, provider);
        }


        public static void CommandConsumer<TCommand,TResult>(
            this IReceiveEndpointConfigurator configuration,
            IServiceProvider provider, 
            Action<CommandExceptionHandlingOptions> setupAction = null) 
            where TCommand : class,ICommand
        {
            configuration.Consumer<CommandRequestConsumer<TCommand,TResult>>(provider, op =>
            {
                op.UseRescue(CommandConsumerRescueContextFactory.Create, cfg =>
                {
                    cfg.UseCommandExceptionHandling(setupAction);
                    cfg.UseCommandExceptionRespond();
                });
            });
        }

        private static void CallGenericCommandConsumer(
            Type commandType,
            Type commandResultType,
            IReceiveEndpointConfigurator configuration,
            IServiceProvider provider,
            Action<CommandExceptionHandlingOptions> setupAction)
        {
            var commandConsumerMethod = typeof(EndpointConfigurationExtensions)
                .GetMethods()
                .Single(m => m.Name == "CommandConsumer" && m.GetGenericArguments().Length == 2)
                .MakeGenericMethod(commandType, commandResultType);

            var configurationParameter = Expression.Parameter(typeof(IReceiveEndpointConfigurator), "configuration");
            var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "provider");
            var setupActionParameter = Expression.Parameter(typeof(Action<CommandExceptionHandlingOptions>), "setupAction");

            var body = Expression.Call(
                commandConsumerMethod, 
                configurationParameter, 
                serviceProviderParameter,
                setupActionParameter);

            var lambda =
                Expression
                    .Lambda<Action<IReceiveEndpointConfigurator, IServiceProvider,
                        Action<CommandExceptionHandlingOptions>>>(body, configurationParameter, serviceProviderParameter,
                        setupActionParameter);
            var action = lambda.Compile();

            action(configuration, provider, setupAction);
        }
    }
}