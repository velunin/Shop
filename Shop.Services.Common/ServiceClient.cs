using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Rds.Cqrs.Commands;
using Shop.Infrastructure.Messaging.MessageContracts;

namespace Shop.Services.Common
{
    public class ServiceClient : IServiceClient
    {
        private readonly ConcurrentDictionary<Type, object> _requestClientsCache = new ConcurrentDictionary<Type, object>();

        private readonly IBus _bus;

        private const int DefaultTimeoutInSec = 60;

        public ServiceClient(IBus bus)
        {
            _bus = bus;
        }

        public async Task ProcessAsync<TCommand>(
            TCommand command, 
            TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class,ICommand
        {
            await SendCommand<TCommand, EmptyResult>(command, timeout, cancellationToken).ConfigureAwait(false);
        }

        public Task ProcessAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            return ProcessAsync(command, TimeSpan.FromSeconds(DefaultTimeoutInSec), cancellationToken);
        }

        public async Task<TResult> ProcessAsync<TCommand, TResult>(
            TCommand command, 
            TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class,IResultingCommand<TResult>
        {
            var response = await SendCommand<TCommand, TResult>(command, timeout, cancellationToken).ConfigureAwait(false);

            return response.Result;
        }

        public Task<TResult> ProcessAsync<TCommand, TResult>(TCommand command,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, IResultingCommand<TResult>
        {
            return ProcessAsync<TCommand, TResult>(command, TimeSpan.FromSeconds(DefaultTimeoutInSec), cancellationToken);
        }

        private async Task<CommandResponse<TResult>> SendCommand<TCommand, TResult>(
            TCommand command, 
            TimeSpan timeout, 
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var client = GetRequestClient<TCommand>();

            var response = (await client
                    .Create(command, cancellationToken, RequestTimeout.After(ms: (int)timeout.TotalMilliseconds))
                    .GetResponse<CommandResponse<TResult>>()
                    .ConfigureAwait(false))
                .Message;

            if (response.ErrorCode.HasValue)
            {
                throw new ServiceException(response.ErrorMessage, response.ErrorCode.Value);
            }

            return response;
        }

        private IRequestClient<TCommand> GetRequestClient<TCommand>() where TCommand : class
        {
            return (IRequestClient<TCommand>)_requestClientsCache.GetOrAdd(typeof(TCommand), CreateRequestClient<TCommand>());
        }

        private IRequestClient<TCommand> CreateRequestClient<TCommand>() where TCommand : class
        {
            //TODO:mapping command type to queue name
            return _bus.CreateClientFactory().CreateRequestClient<TCommand>(new Uri($"rabbitmq://127.0.0.1/{ServicesQueues.OrderServiceCommandQueue}"));
        }
    }
}