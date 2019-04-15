using System;
using System.Threading;
using System.Threading.Tasks;
using MassInstance.MessageContracts;

namespace MassInstance.Bus
{
    internal class MassInstanceRequestHandle<TResult> : IRequestHandleResponseSetter
    {
        private readonly TaskCompletionSource<CommandResponse<TResult>> _completionSource =
            new TaskCompletionSource<CommandResponse<TResult>>();

        public void SetResponse(object response)
        {
            if (!(response is CommandResponse<TResult> commandResponse))
            {
                throw new InvalidOperationException(
                    $"Command response must be {typeof(CommandResponse<TResult>)} type");
            }

            _completionSource.TrySetResult(commandResponse);
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
}