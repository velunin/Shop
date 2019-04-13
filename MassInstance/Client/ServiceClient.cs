using System;
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

    public class ServiceClientNew : IServiceClient, ICallbackReceiver
    {
        private readonly ConcurrentDictionary<Guid, IResponseSetter> _handleResponseSetters = new ConcurrentDictionary<Guid, IResponseSetter>();

        private readonly IBus _bus;
        private readonly IQueuesMapper _mapper;
        private readonly ILogger _logger;
        private readonly BrokerConfig _brokerConfig;

        private const int DefaultTimeoutInSec = 30;

        public ServiceClientNew(
            IBus bus, 
            IQueuesMapper mapper, 
            ILogger logger, 
            BrokerConfig brokerConfig)
        {
            _bus = bus;
            _mapper = mapper;
            _logger = logger;
            _brokerConfig = brokerConfig;
        }

        public Task ProcessAsync<TCommand>(TCommand command, TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            return SendCommand<TCommand, EmptyResult>(command, timeout, cancellationToken);
        }

        public Task ProcessAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            return SendCommand<TCommand, EmptyResult>(command, TimeSpan.FromSeconds(DefaultTimeoutInSec), cancellationToken);
            throw new NotImplementedException();
        }

        public Task<TResult> ProcessAsync<TCommand, TResult>(TCommand command, TimeSpan timeout,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, IResultingCommand<TResult>
        {
            return SendCommand<TCommand, TResult>(command, timeout, cancellationToken);
            throw new NotImplementedException();
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
                        _logger.LogDebug($"Try remove request handle {requestId}");

                        if (_handleResponseSetters.TryRemove(requestId, out _))
                        {
                            _logger.LogDebug($"Request {requestId} was removed");
                        }
                    },
                    cancellationToken);

            await _bus.Send(command, cancellationToken);

            var responseAndDeleteTask = Task.WhenAll(responseTask, deleteHandleTask);
            var completedTask = await Task.WhenAny(responseAndDeleteTask, delayedTask);

            if (completedTask != responseAndDeleteTask)
            {
                requestHandle.SetCancelled();
                throw new TimeoutException("Request timed out");
            }

            var response = await responseTask;

            if (response.ErrorCode.HasValue)
            {
                _logger.LogDebug($"Error response for command: {typeof(TCommand)}\r\n" +
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
    }

    public interface ICallbackReceiver
    {
        void ResponseCallback(Guid requestId, object response);
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

   internal interface IResponseSetter
   {
       void SetResponse(object response);
   }
}