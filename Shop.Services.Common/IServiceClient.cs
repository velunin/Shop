using System;
using System.Threading;
using System.Threading.Tasks;
using Rds.Cqrs.Commands;

namespace Shop.Services.Common
{
    public interface IServiceClient
    {
        Task ProcessAsync<TCommand>(
            TCommand command, 
            TimeSpan timeout, 
            CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : class, ICommand;

        Task ProcessAsync<TCommand>(
            TCommand command,
            CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : class, ICommand;

        Task<TResult> ProcessAsync<TCommand, TResult>(
            TCommand command, 
            TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : class, IResultingCommand<TResult>;

        Task<TResult> ProcessAsync<TCommand, TResult>(
            TCommand command,
            CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : class, IResultingCommand<TResult>;
    }
}