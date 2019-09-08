using MassInstance.Configuration.ServiceMap;
using Shop.Order.Domain.Commands;

namespace Shop.Shared.Services
{
    public class CartServiceMap : IServiceMap
    {

    }

    public class OrderServiceMap : IServiceMap
    {
        public CommandQueueMap OrderCommands;

        public class CommandQueueMap : IQueueMap
        {
            public CreateOrderCommand CreateOrderCommand;
        }
    }
}