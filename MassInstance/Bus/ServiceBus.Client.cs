using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MassInstance.Client;
using MassInstance.Configuration.Client;
using MassInstance.Cqrs.Commands;
using MassInstance.MessageContracts;

namespace MassInstance.Bus
{
    public partial class ServiceBus
    {
        private readonly ConcurrentDictionary<Guid, IRequestHandleResponseSetter> _handleResponseSetters = new ConcurrentDictionary<Guid, IRequestHandleResponseSetter>();

        private readonly IQueuesMapper _mapper;
        private readonly SerivceClientConfig _serivceClientConfig;

        private const int DefaultTimeoutInSec = 30;

        public ServiceBus(
            IQueuesMapper mapper,
            SerivceClientConfig serivceClientConfig)
        {
            _mapper = mapper;
            _serivceClientConfig = serivceClientConfig;
        }

        public Task ProcessAsync<TCommand>(TCommand command, TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            return SendCommand<TCommand, EmptyResult>(command, timeout, cancellationToken);
        }

        public Task ProcessAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            return SendCommand<TCommand, EmptyResult>(command, TimeSpan.FromSeconds(DefaultTimeoutInSec), cancellationToken);
        }

        public Task<TResult> ProcessAsync<TCommand, TResult>(TCommand command, TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, IResultingCommand<TResult>
        {
            return SendCommand<TCommand, TResult>(command, timeout, cancellationToken);
        }

        public Task<TResult> ProcessAsync<TCommand, TResult>(TCommand command,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, IResultingCommand<TResult>
        {
            return SendCommand<TCommand, TResult>(command, TimeSpan.FromSeconds(DefaultTimeoutInSec), cancellationToken);
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

            await sendEndpoint.Send(command, cancellationToken);

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
            var host = _serivceClientConfig.BrokerUri.Trim().TrimEnd('/');

            return new Uri($"{host}/{queue}");
        }
    }

    public interface IResponseReceiver
    {
        void ResponseCallback(Guid requestId, object response);
    }

    internal class MassInstanceRequestHandle<TResult> : IRequestHandleResponseSetter
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

        public Task<CommandResponse<TResult>> GetResponse(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => _completionSource.TrySetCanceled(), false);

            return _completionSource.Task;
        }

        public void SetCancelled()
        {
            _completionSource.SetCanceled();
        }
    }

    internal interface IRequestHandleResponseSetter
    {
        void SetResponse(object response);
    }
}
