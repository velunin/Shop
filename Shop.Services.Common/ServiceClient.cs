using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Options;

using Rds.Cqrs.Commands;

using Shop.DataAccess.Dto;
using Shop.Services.Common.MessageContracts;

namespace Shop.Services.Common
{
    public class ServiceClient : IServiceClient
    {
        private readonly ConcurrentDictionary<Type, object> _requestClientsCache = new ConcurrentDictionary<Type, object>();

        private readonly IBus _bus;
        private readonly IQueuesMapper _mapper;
        private readonly RabbitMqConfig _config;

        private const int DefaultTimeoutInSec = 15;

        public ServiceClient(
            IBus bus, 
            IQueuesMapper mapper, 
            IOptions<RabbitMqConfig> config)
        {
            _bus = bus;
            _mapper = mapper;
            _config = config.Value;
        }

        public async Task ProcessAsync<TCommand>(
            TCommand command, 
            TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class,ICommand
        {
            await SendCommand<TCommand, EmptyResult>(command, timeout, cancellationToken).ConfigureAwait(false);
        }

        public Task ProcessAsync<TCommand>(
            TCommand command,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
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

            var requestTimeout = RequestTimeout.After(ms: (int) timeout.TotalMilliseconds);
            var response = (await client
                    .Create(
                        command,
                        cancellationToken,
                        requestTimeout)
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
            return _bus.CreateRequestClient<TCommand>(
                BuildUriForCommand<TCommand>());
        }

        private Uri BuildUriForCommand<TCommand>()
        {
            var queue = _mapper.GetQueueName<TCommand>();
            var host = _config.Uri.Trim().TrimEnd('/');

            return new Uri($"{host}/{queue}");
        }
    }
}