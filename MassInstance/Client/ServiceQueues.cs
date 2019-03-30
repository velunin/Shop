
namespace MassInstance.Client
{
    public class ServicesQueues
    {
        public const string OrderServiceCommandQueue = "order-service-commands";
        public const string OrderServiceSagaQueue = "order-service-saga";
        public const string OrderServiceEventsQueue = "order-service-events";

        public const string CartServiceCommandsQueue = "cart-service-commands";
    }
}