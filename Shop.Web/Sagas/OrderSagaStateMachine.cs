using System;
using Automatonymous;
using Automatonymous.Binders;
using Rds.Cqrs.Commands;
using Shop.Domain.Commands.Order;

namespace Shop.Web.Sagas
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
                When(AddOrderContacts)
                    .ThenAsync(async context =>
                    {

                        await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);
                    }));
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
    }
}