using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Rds.Cqrs.Commands;
using Shop.Services.Common.MessageContracts;

namespace Shop.Infrastructure.Messaging
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
            _logger.LogInformation($"Receive command: {typeof(TCommand)}. Result type: {typeof(TResult)}");

            var result = default(TResult);

            if (typeof(TResult) == typeof(EmptyResult))
            {
                await _commandProcessor.ProcessAsync(context.Message, context.CancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                result = await _commandProcessor.ProcessAsync((IResultingCommand<TResult>)context.Message, context.CancellationToken)
                    .ConfigureAwait(false);
            }

            await context.RespondAsync(new CommandResponse<TResult>(result)).ConfigureAwait(false);
        }
    }
}