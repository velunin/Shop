using System;
using System.Threading;
using System.Threading.Tasks;
using RDS.CaraBus;

namespace Rds.CaraBus.RequestResponse
{
    internal class RequestClient<TRequest, TResponse> : 
        IRequestClient<TRequest, TResponse>
        where TResponse : class
        where TRequest : class
    {
        private const int DefaultTimeoutInSeconds = 10;

        private readonly ICaraBus _caraBus;
        private readonly ICorrelationValueResolver _correaltionValueResolver;

        public RequestClient(
            ICaraBus caraBus,
            ICorrelationValueResolver correaltionValueResolver)
        {
            _caraBus = caraBus;
            _correaltionValueResolver = correaltionValueResolver;
        }


        public Task<TResponse> Request(TRequest request, CancellationToken cancellationToken)
        {
            return Request(request, TimeSpan.FromSeconds(DefaultTimeoutInSeconds), cancellationToken);
        }

        public async Task<TResponse> Request(
            TRequest request,
            TimeSpan requestTimeout,
            CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<TResponse>();

            cancellationToken.Register(() => completionSource.TrySetCanceled(), false);

            _caraBus.Subscribe<TResponse>(message =>
            {
                try
                {
                    if (_correaltionValueResolver.IsCorrelate(request, message))
                    {
                        completionSource.SetResult(message);
                    }
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            });

            _caraBus.Subscribe<Fault<TRequest>>(message =>
            {
                try
                {
                    if (_correaltionValueResolver.IsCorrelate(request, message.Request))
                    {
                        completionSource.SetException(message.Exception);
                    }
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            });

            await _caraBus.PublishAsync(request, cancellationToken: cancellationToken);

            var delayedTask = Task.Delay(requestTimeout, cancellationToken);

            var completedTask = await Task.WhenAny(completionSource.Task, delayedTask);

            if (completedTask != completionSource.Task)
            {
                completionSource.SetCanceled();

                throw new TimeoutException("Request timed out");
            }

            return await completionSource.Task;
        }
    }
}