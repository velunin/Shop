using System;
using System.Threading.Tasks;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using Rds.Cqrs.Commands;
using Shop.Infrastructure.Messaging.MessageContracts;

namespace Shop.Infrastructure.Messaging.Extensions
{
    public static class CommandConfigurationExtensions
    {
        public static void UseCommandExceptionHandling<TConsumer>(
            this IRescueConfigurator<ConsumerConsumeContext<TConsumer>, CommandConsumerRescueContext<TConsumer>>
                configurator,
            Action<CommandExceptionHandlingOptions> setupOptions)
            where TConsumer : class
        {
            configurator.UseExecute(context =>
            {
              
                if (!context.ReceiveContext.TryGetPayload(out CommandExceptionHandlingOptions handlingOptions))
                {
                    handlingOptions = new CommandExceptionHandlingOptions();
                }

                setupOptions?.Invoke(handlingOptions);
            });
        }

        public static void UseCommandExceptionRespond<TCommand, TResult>(
            this IRescueConfigurator<ConsumerConsumeContext<CommandRequestConsumer<TCommand, TResult>>,
                CommandConsumerRescueContext<CommandRequestConsumer<TCommand, TResult>>> configuration,
            Action<CommandExceptionHandlingOptions> setupAction = null)
            where TCommand : class, ICommand
        {
            configuration.UseExecuteAsync(async context =>
            {
                if (context.ReceiveContext.TryGetPayload(out CommandExceptionHandlingOptions options))
                {
                    var exceptionResponse = options.GetResponse(context.Exception);

                    await context
                        .RespondAsync(
                            new CommandResponse<TResult>(
                                exceptionResponse.Code,
                                exceptionResponse.Message))
                        .ConfigureAwait(false);
                }
                else
                {
                    throw context.Exception;
                }
            });
        }

        public static void UseCommandExceptionHandling(this IReceiveEndpointConfigurator configuration, Action<CommandExceptionHandlingOptions> setupAction = null)
        {
            configuration.UseContextFilter(context =>
            {
                var options = new CommandExceptionHandlingOptions();

                setupAction?.Invoke(options);

                context.AddOrUpdatePayload(() => options, x => options);

                return Task.FromResult(true);
            });
        }
    }
}