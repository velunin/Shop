using System;
using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using MassInstance.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace MassInstance.Messaging
{
    public class CommandRequestConsumer<TCommand,TResult> : IConsumer<TCommand>
        where TCommand : class, ICommand   
    {
        private readonly ICommandProcessor _commandProcessor;
        private readonly ILogger _logger;

        public CommandRequestConsumer(
            ICommandProcessor commandProcessor,
            ILogger<CommandRequestConsumer<TCommand, TResult>> logger)
        {
            _commandProcessor = commandProcessor;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TCommand> context)
        {
            LogDebugInfo(context);

            try
            {
                var result = default(TResult);

                if (typeof(TResult) == typeof(EmptyResult))
                {
                    await _commandProcessor.ProcessAsync(context.Message, context.CancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    result = await _commandProcessor.ProcessAsync(
                            (IResultingCommand<TResult>) context.Message,
                            context.CancellationToken)
                        .ConfigureAwait(false);
                }

                await context.RespondAsync(new CommandResponse<TResult>(result)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Command processing error");

                if (ExceptionResponseResolver.TryResolveResponse(
                    typeof(TCommand), 
                    ex, 
                    out var exceptionResponse))
                {
                    await context
                        .RespondAsync(
                            new CommandResponse<TResult>(
                                exceptionResponse.Code,
                                exceptionResponse.Message))
                        .ConfigureAwait(false);
                }
                else throw;
            }
        }

        private void LogDebugInfo(MessageContext context)
        {
            _logger.LogDebug($"Receive command: {typeof(TCommand)}\r\n" +
                             $"Result type: {typeof(TResult)}\r\n" +
                             $"{(context.RequestId.HasValue ? $"RequestId: {context.RequestId.Value}" : string.Empty)}");
        }
    }
}