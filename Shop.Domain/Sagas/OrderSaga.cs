using System;
using System.Collections.Generic;

using NSaga;

using Rds.Cqrs.Commands;

using Shop.DataProjections.Models;
using Shop.Domain.Commands.Order;
using Shop.Domain.Events;
using Shop.DataProjections.SagasContexts;

namespace Shop.Domain.Sagas
{
    public class OrderSaga : 
        ISaga<OrderSagaContext>, 

        InitiatedBy<CreateOrderMessage>,

        ConsumerOf<OrderCreated>,
        ConsumerOf<AddOrderContactsMessage>,
        ConsumerOf<OrderContactsAdded>
    {
        private readonly ICommandProcessor _commandProcessor;

        public OrderSaga(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }

        public Guid CorrelationId { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public OrderSagaContext SagaData { get; set; }

        public OperationResult Initiate(CreateOrderMessage message)
        {
            _commandProcessor.Process(message.Command);

            SagaData.OrderId = message.Command.OrderId;
            SagaData.CurrentOrderStatus = OrderStatus.New; 

            return new OperationResult();
        }

        public OperationResult Consume(OrderCreated message)
        {
            return new OperationResult();
        }

        public OperationResult Consume(AddOrderContactsMessage message)
        {
            _commandProcessor.Process(message.Command);

            return new OperationResult();
        }

        public OperationResult Consume(OrderContactsAdded message)
        {
            return new OperationResult();
        }
    }
}