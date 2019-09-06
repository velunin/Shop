using MassInstance.Configuration.ServiceMap;
using Shop.Cart.Domain.Commands;
using Shop.Order.Domain.Commands;

namespace Shop.Services.Shared
{
    public class CartServiceMap : IServiceMap
    {
        public CommandsQueueMap CartServiceCommands;

        public class CommandsQueueMap : IQueueMap
        {
            public AddOrUpdateProductInCart AddOrUpdateProductInCart;
            public DeleteProductFromCart DeleteProductFromCart;
        }
    }

    public class OrderServiceMap : IServiceMap
    {
        public SagaQueue OrderServiceSaga;

        public class SagaQueue : IQueueMap
        {
            public CreateOrderCommand CreateOrderCommand;
            public AddOrderContactsCommand AddOrderContactCommand;
            public PayOrderCommand PayOrderCommand;
        }
    }
}