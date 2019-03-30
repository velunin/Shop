using Shop.Cqrs.Commands;
using Shop.Domain.Commands.Cart;
using Shop.Domain.Commands.Order;

namespace Shop.Services.Common
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


    public interface IServiceMap { }

    public interface IQueueMap { }
}