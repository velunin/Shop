﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MassInstance.Configuration.Client;
using MassInstance.Cqrs.Commands;
using MassInstance.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace MassInstance.Client
{
    public class ServiceClient : IServiceClient
    {
        private readonly ConcurrentDictionary<Type, object> _requestClientsCache = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Guid,IResponseSetter> _handleResponseSetters = new ConcurrentDictionary<Guid, IResponseSetter>();

        private readonly IBus _bus;
        private readonly IQueuesMapper _mapper;
        private readonly ILogger _logger;
        private readonly BrokerConfig _brokerConfig;

        private const int DefaultTimeoutInSec = 30;

        public ServiceClient(
            IBus bus, 
            IQueuesMapper mapper,
            ILogger<ServiceClient> logger,
            BrokerConfig brokerConfig)
        {
            _bus = bus;
            _mapper = mapper;
            _logger = logger;
            _brokerConfig = brokerConfig;
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
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, IResultingCommand<TResult>
        {
            var response = await SendCommand<TCommand, TResult>(command, timeout, cancellationToken).ConfigureAwait(false);

            return response.Result;
        }

        public Task<TResult> ProcessAsync<TCommand, TResult>(TCommand command,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, IResultingCommand<TResult>
        {
            return ProcessAsync<TCommand, TResult>(command, TimeSpan.FromSeconds(DefaultTimeoutInSec), cancellationToken);
        }

        public void SetResponse(Guid requestId, object response)
        {
            if(_handleResponseSetters.TryGetValue(requestId, out var setter))
            {
                setter.SetResponse(response);
            }
        }

        private async Task<CommandResponse<TResult>> SendCommand<TCommand, TResult>(
            TCommand command, 
            TimeSpan timeout, 
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var client = GetRequestClient<TCommand>();

            var requestTimeout = RequestTimeout.After(ms: (int) timeout.TotalMilliseconds);
            var requestHandle = client
                .Create(
                    command,
                    cancellationToken,
                    requestTimeout);

           _logger.LogDebug($"Send command: {typeof(TCommand)}\r\n" +
                            $"RequestId: {requestHandle.RequestId}");

            var response = (await requestHandle
                    .GetResponse<CommandResponse<TResult>>()
                    .ConfigureAwait(false))
                .Message;

            if (response.ErrorCode.HasValue)
            {
                _logger.LogDebug($"Error response for command: {typeof(TCommand)}\r\n" +
                                 $"RequestId: {requestHandle.RequestId}\r\n" +
                                 $"ErrorCode: {response.ErrorCode.Value}, Message: {response.ErrorMessage}");

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
            var host = _brokerConfig.Uri.Trim().TrimEnd('/');

            return new Uri($"{host}/{queue}");
        }
    }

   internal class MassInstanceRequestHandle<TResult> : IResponseSetter
    {
        private readonly TaskCompletionSource<CommandResponse<TResult>> _completionSource = new TaskCompletionSource<CommandResponse<TResult>>();

        public void SetResponse(object response)
        {
            if (!(response is CommandResponse<TResult> commandResponse))
            {
                throw new InvalidOperationException($"Command response must be {typeof(CommandResponse<TResult>)} type");
            }

            _completionSource.SetResult(commandResponse);
        }

        public Task<CommandResponse<TResult>> GetResponse()
        {
            return _completionSource.Task;
        }
    }

   internal interface IResponseSetter
   {
       void SetResponse(object response);
   }
}