using Shop.Cqrs.Commands;
using Shop.Domain.Commands.Cart;
using Shop.Domain.Commands.Order;

namespace Shop.Services.Common
{
    public class CartServiceMap
    {
        public Queue<CommandsQueueMap> CartServiceCommands;

        public class CommandsQueueMap
        {
            public Command<AddOrUpdateProductInCart> AddOrUpdateProductInCart;
            public Command<DeleteProductFromCart> DeleteProductFromCart;
        }
    }

    public class OrderServiceMap
    {
        public Queue<SagaQueue> OrderServiceSaga;

        public class SagaQueue
        {
            public Command<CreateOrderCommand> CreateOrderCommand;
            public Command<AddOrderContactsCommand> AddOrderContactCommand;
            public Command<PayOrderCommand> PayOrderCommand;
        }
    }

    public class Service<TServiceMap> : IService
    {
    }

    public class Queue<TQueueMap> : IQueue
    {
    }

    public class Command<TCommand> where TCommand : ICommand
    {
    }

    public interface IService { }

    public interface IQueue { }
}