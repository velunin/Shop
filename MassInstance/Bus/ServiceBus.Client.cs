using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MassInstance.Client;
using MassInstance.Configuration.Client;
using MassInstance.Cqrs.Commands;
using MassInstance.MessageContracts;
using MassTransit;

namespace MassInstance.Bus
{
    public partial class ServiceBus
    {
        private readonly ConcurrentDictionary<Guid, IRequestHandleResponseSetter> _handleResponseSetters =
            new ConcurrentDictionary<Guid, IRequestHandleResponseSetter>();

        private readonly IQueuesMapper _mapper;
        private readonly SerivceClientConfig _serivceClientConfig;

        private const int DefaultTimeoutInSec = 30;

        public ServiceBus(
            IBusControl busControl,
            IQueuesMapper mapper,
            SerivceClientConfig serivceClientConfig)
        {
            _busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _serivceClientConfig = serivceClientConfig ?? throw new ArgumentNullException(nameof(serivceClientConfig));
        }

        public Task ProcessAsync<TCommand>(TCommand command, TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            return SendCommand<TCommand, EmptyResult>(command, timeout, cancellationToken);
        }

        public Task ProcessAsync<TCommand>(TCommand command,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            return SendCommand<TCommand, EmptyResult>(command, TimeSpan.FromSeconds(DefaultTimeoutInSec),
                cancellationToken);
        }

        public Task<TResult> ProcessAsync<TCommand, TResult>(TCommand command, TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : class, IResultingCommand<TResult>
        {
            return SendCommand<TCommand, TResult>(command, timeout, cancellationToken);
        }

        public Task<TResult> ProcessAsync<TCommand, TResult>(TCommand command,
            CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : class, IResultingCommand<TResult>
        {
            return SendCommand<TCommand, TResult>(command, TimeSpan.FromSeconds(DefaultTimeoutInSec),
                cancellationToken);
        }

        private async Task<TResult> SendCommand<TCommand, TResult>(
            TCommand command,
            TimeSpan timeout,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var requestId = Guid.NewGuid();

            var requestHandle = new MassInstanceRequestHandle<TResult>();

            _handleResponseSetters.AddOrUpdate(requestId, requestHandle, (_, setter) => setter);

            var delayedTask = Task.Delay(timeout, cancellationToken);
            var responseTask = requestHandle.GetResponse(cancellationToken);

            var deleteHandleTask =
                responseTask.ContinueWith(task =>
                    {
                        _log.Debug($"Try remove request handle {requestId}");

                        if (_handleResponseSetters.TryRemove(requestId, out _))
                        {
                            _log.Debug($"Request {requestId} was removed");
                        }
                    },
                    cancellationToken);

            var sendEndpoint = await GetSendEndpoint(BuildUriForCommand<TCommand>());

            await sendEndpoint.Send(command,context=>
            {
                context.RequestId = requestId;
                context.ResponseAddress = new Uri(
                    _serivceClientConfig.BrokerUri, 
                    _serivceClientConfig.CallbackQueue);
            }, cancellationToken);

            var responseWithDeleteHandleTask = Task.WhenAll(responseTask, deleteHandleTask);

            if (responseWithDeleteHandleTask != await Task.WhenAny(responseWithDeleteHandleTask, delayedTask))
            {
                requestHandle.SetCancelled();

                throw new TimeoutException("Request timed out");
            }

            var response = await responseTask;

            if (response.ErrorCode.HasValue)
            {
                _log.Debug($"Error response for command: {typeof(TCommand)}\r\n" +
                           $"RequestId: {requestId}\r\n" +
                           $"ErrorCode: {response.ErrorCode.Value}, Message: {response.ErrorMessage}");

                throw new ServiceException(response.ErrorMessage, response.ErrorCode.Value);
            }

            return response.Result;
        }

        public void ResponseCallback(Guid requestId, object response)
        {
            if (_handleResponseSetters.TryGetValue(requestId, out var setter))
            {
                setter.SetResponse(response);
            }
        }

        private Uri BuildUriForCommand<TCommand>()
        {
            var queue = _mapper.GetQueueName<TCommand>();

            return new Uri(_serivceClientConfig.BrokerUri, queue);
        }
    }
}
