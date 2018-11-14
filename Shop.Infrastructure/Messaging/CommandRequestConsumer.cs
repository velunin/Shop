using System.Threading.Tasks;
using MassTransit;
using Rds.Cqrs.Commands;
using Shop.Infrastructure.Messaging.MessageContracts;

namespace Shop.Infrastructure.Messaging
{
    public class CommandRequestConsumer<TCommand,TResult> : IConsumer<TCommand>
        where TCommand : class, ICommand   
    {
        private readonly ICommandProcessor _commandProcessor;

        public CommandRequestConsumer(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }

        public async Task Consume(ConsumeContext<TCommand> context)
        {
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