using System.Threading.Tasks;
using MassInstance.Client;
using MassInstance.MessageContracts;
using MassTransit;

namespace MassInstance.Messaging
{
    public class CallbackCommandConsumer<TResponse, TResult> : IConsumer<TResponse> 
        where TResponse : CommandResponse<TResult>
    {
        private readonly IServiceClient _serviceClient;

        public CallbackCommandConsumer(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public Task Consume(ConsumeContext<TResponse> context)
        {
            if (context.RequestId.HasValue)
            {
                _serviceClient.ResponseCallback(context.RequestId.Value, context.Message);
            }

            return Task.CompletedTask;
        }
    }
}