using System;
using Automatonymous;
using Automatonymous.Binders;
using Microsoft.Extensions.Logging;
using Rds.Cqrs.Commands;
using Shop.DataAccess.Dto;
using Shop.Domain.Commands.Order;
using Shop.Services.Common.MessageContracts;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Shop.Services.Order.Sagas
{
    public class OrderSagaStateMachine : MassTransitStateMachine<OrderSaga>
    {
        public State OrderCreated { get; private set; }

        public State OrderCreationError { get; private set; }

        public Event<CreateOrderCommand> CreateOrder { get; private set; }

        public Event<AddOrderContactsCommand> AddOrderContacts { get; private set; }

        public OrderSagaStateMachine(ICommandProcessor commandProcessor, ILogger<OrderSagaStateMachine> logger)
        {
            InstanceState(x => x.CurrentState);

            Event(() => CreateOrder, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => AddOrderContacts, x => x.CorrelateById(o => o.Message.CorrelationId));

            Initially(WhenCreateOrderCommand(commandProcessor, logger));
                
            During(OrderCreated,
                WhenAddOrderContactsCommand(commandProcessor, logger));
        }

        private EventActivityBinder<OrderSaga, CreateOrderCommand> WhenCreateOrderCommand(
            ICommandProcessor commandProcessor, ILogger<OrderSagaStateMachine> logger)
        {
            return When(CreateOrder)
                .ThenAsync(async context =>
                {
                    logger.LogInformation($"Receive command in saga: {typeof(OrderSaga)}");
                    await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);
                })
                .Respond(new CommandResponse<EmptyResult>()).TransitionTo(OrderCreated)
                .Catch<Exception>(binder =>
                    binder
                        .Respond(x => new CommandResponse<EmptyResult>(0, "Unknown error from saga response"))
                        .Then(c =>
                        {
                            //Compensative logic
                            logger.LogInformation("Do compensative actions");
                        })
                        .TransitionTo(OrderCreationError))
                ;
        }

        private EventActivityBinder<OrderSaga, AddOrderContactsCommand> WhenAddOrderContactsCommand(
            ICommandProcessor commandProcessor, ILogger<OrderSagaStateMachine> logger)
        {
            return When(AddOrderContacts)
                .ThenAsync(async context =>
                {
                    await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);
                });
        }
    }
}