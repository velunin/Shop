using System;
using Automatonymous;
using Automatonymous.Binders;
using Rds.Cqrs.Commands;
using Shop.Domain.Commands.Order;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Shop.Services.Order.Sagas
{
    public class OrderSagaStateMachine : MassTransitStateMachine<OrderSaga>
    {
        public State OrderCreated { get; private set; }

        public State OrderCreationError { get; private set; }

        public Event<CreateOrderCommand> CreateOrder { get; private set; }

        public Event<AddOrderContactsCommand> AddOrderContacts { get; private set; }

        public OrderSagaStateMachine(ICommandProcessor commandProcessor)
        {
            InstanceState(x => x.CurrenState);

            Event(() => CreateOrder, x => x.CorrelateById(o => o.Message.CorrelationId));

            Initially(WhenCreateOrderCommand(commandProcessor));
                
            During(OrderCreated,
                WhenAddOrderContactsCommand(commandProcessor));
        }

        private EventActivityBinder<OrderSaga, CreateOrderCommand> WhenCreateOrderCommand(
            ICommandProcessor commandProcessor)
        {
            return When(CreateOrder)
                .ThenAsync(async context =>
                {
                    await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);
                })
                .Catch<Exception>(binder =>
                    binder
                        .Then(c =>
                        {

                        })
                        .TransitionTo(OrderCreationError)
                        .Finalize())
                .TransitionTo(OrderCreated);
        }

        private EventActivityBinder<OrderSaga, AddOrderContactsCommand> WhenAddOrderContactsCommand(
            ICommandProcessor commandProcessor)
        {
            return When(AddOrderContacts)
                .ThenAsync(async context =>
                {
                    await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);
                });
        }
    }
}