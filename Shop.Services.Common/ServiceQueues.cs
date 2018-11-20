
namespace Shop.Services.Common
{
    public class ServicesQueues
    {
        public const string OrderServiceCommandQueue = "order-service-commands";
        public const string OrderServiceSagaQueue = "order-service-saga";
        public const string OrderServiceEventsQueue = "order-service-events";
    }
}