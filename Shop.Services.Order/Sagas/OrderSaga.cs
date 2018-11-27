using System;

using Automatonymous;

using Microsoft.Extensions.Logging;

using Rds.Cqrs.Commands;

using Shop.DataAccess.Dto;
using Shop.Domain.Commands.Order;
using Shop.Domain.Commands.Order.Results;
using Shop.Domain.Events;
using Shop.Services.Common.MessageContracts;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Shop.Services.Order.Sagas
{
    public class OrderSaga : MassTransitStateMachine<OrderSagaContext>
    {
        public State OrderCreated { get; private set; }

        public State OrderCreating { get; private set; }

        public State OrderCreationError { get; private set; }

        public State CustomerContactsRecieved { get; private set; }

        public State OrderHasBeenPayed { get; private set; }

        public Event<CreateOrderCommand> CreateOrder { get; private set; }

        public Event<OrderCreated> OrderCreatedEvent { get; private set; }

        public Event<AddOrderContactsCommand> AddOrderContacts { get; private set; }

        public Event<PayOrderCommand> PayOrder { get; private set; }

        public Event<OrderHasBeenPayed> OrderHasBeenPayedEvent { get; private set; }

        public OrderSaga(ICommandProcessor commandProcessor, ILogger<OrderSaga> logger, IFakeMailService mailService)
        {
            InstanceState(x => x.CurrentState);

            Event(() => CreateOrder, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => AddOrderContacts, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => OrderCreatedEvent, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => PayOrder, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => OrderHasBeenPayedEvent, x => x.CorrelateById(o => o.Message.CorrelationId));

            Initially(
                When(CreateOrder)
                    .ThenAsync(async context =>
                    {
                        logger.LogInformation($"Receive command in saga: {typeof(OrderSagaContext)}");

                        await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);
                    })
                    .Respond(new CommandResponse<EmptyResult>())
                    .TransitionTo(OrderCreating)
                    .Catch<Exception>(binder =>
                        binder
                            .Respond(x => new CommandResponse<EmptyResult>(0, "Unknown error from saga response"))
                            .Then(c =>
                            {
                                //Compensative logic
                                logger.LogInformation("Do compensative actions");
                            })
                            .TransitionTo(OrderCreationError)));

            During(OrderCreating,
                When(OrderCreatedEvent)
                    .TransitionTo(OrderCreated));

            During(OrderCreated,
                When(AddOrderContacts)
                    .TransitionTo(CustomerContactsRecieved)
                    .ThenAsync(async context =>
                    {
                        await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);

                        context.Instance.Name = context.Data.Name;
                        context.Instance.Email = context.Data.Email;
                        context.Instance.Phone = context.Data.Phone;
                    }));

            During(CustomerContactsRecieved,
                When(PayOrder)
                    .ThenAsync(async context =>
                    {
                        var result = await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);
                        await context.RespondAsync(new CommandResponse<PayOrderResult>(result)).ConfigureAwait(false);
                    }),
                When(OrderHasBeenPayedEvent)
                    .TransitionTo(OrderHasBeenPayed)
                    .ThenAsync(async context =>
                    {
                        await mailService.SendEmailOrder(
                            context.Instance.Name,
                            context.Instance.Email,
                            context.Instance.Phone).ConfigureAwait(false);
                    }).Finalize());
        }
    }
}