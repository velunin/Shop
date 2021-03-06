﻿using System.Threading;
using System.Threading.Tasks;

namespace MassInstance.Cqrs.Commands
{
    public interface ICommandProcessor
    {
        Task ProcessAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken));

        Task<TResult> ProcessAsync<TResult>(IResultingCommand<TResult> command,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}