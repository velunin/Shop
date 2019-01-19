using System.Threading;
using System.Threading.Tasks;

namespace Shop.Cqrs.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface ICommandHandler<in TCommand, TResult> where TCommand : IResultingCommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}