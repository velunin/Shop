using System;
using System.Threading.Tasks;

using Automatonymous;
using MassInstance.Cqrs.Commands;
using MassInstance.MessageContracts;
using MassTransit;

using Microsoft.Extensions.Logging;
using Shop.DataAccess.Dto;
using Shop.Domain.Commands.Order;
using Shop.Domain.Commands.Order.Results;
using Shop.Domain.Events;
using Shop.Services.Common.ErrorCodes;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Shop.Services.Order.Sagas
{
    public class OrderSaga : MassTransitStateMachine<OrderSagaContext>
    {
        public State OrderCreating { get; private set; }

        public State OrderCreated { get; private set; }

        public State OrderCreationError { get; private set; }

        public State CustomerContactsRecieved { get; private set; }

        public State OrderHasBeenPayed { get; private set; }

        public Event<CreateOrderCommand> CreateOrder { get; private set; }

        public Event<OrderCreated> OrderCreatedEvent { get; private set; }

        public Event<AddOrderContactsCommand> AddOrderContacts { get; private set; }

        public Event<PayOrderCommand> PayOrder { get; private set; }

        public Event<OrderHasBeenPayed> OrderHasBeenPayedEvent { get; private set; }

        public Event<OrderCreatingUnknownError> OrderCreatingUnknownErrorEvent { get; private set; }

        public Event<OrderCreatingAlreadySoldError> OrderCreatingAlreadySoldErrorEvent { get; private set; }

        public OrderSaga(
            ICommandProcessor commandProcessor,
            ILogger<OrderSaga> logger,
            IFakeMailService mailService)
        {
            InstanceState(x => x.CurrentState);

            Event(() => CreateOrder, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => AddOrderContacts, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => OrderCreatedEvent, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => OrderCreatingUnknownErrorEvent, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => OrderCreatingAlreadySoldErrorEvent, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => PayOrder, x => x.CorrelateById(o => o.Message.CorrelationId));
            Event(() => OrderHasBeenPayedEvent, x => x.CorrelateById(o => o.Message.CorrelationId));

            Initially(
                When(CreateOrder)
                    .Then(context =>
                    {
                        if (context.TryGetPayload(out ConsumeContext<CreateOrderCommand> consumeContext))
                        {
                            logger.LogDebug($"Receive command: {consumeContext.Message.GetType()}\r\n" +
                                            $"RequestId: {consumeContext.RequestId}\r\n" +
                                            $"CorrelationId: {consumeContext.Message.CorrelationId}");

                            context.Instance.SetCreateOrderRequestIdentity(
                                consumeContext.RequestId,
                                consumeContext.ResponseAddress.AbsoluteUri);
                        }

                        commandProcessor.ProcessAsync(context.Data);
                    })
                    .TransitionTo(OrderCreating));

            During(OrderCreating,
                When(OrderCreatedEvent)
                    .ThenAsync(async context =>
                    {
                        logger.LogDebug("OrderCreatedEvent}\r\n" +
                                        $"CorrelationId: {context.Data.CorrelationId}");

                        var (requestId, responseAddress) = context.Instance.GetCreateOrderRequestIdentity();

                        if (requestId.HasValue)
                        {
                            await RespondFromSaga(
                                    context,
                                    new CommandResponse<EmptyResult>(),
                                    requestId.Value,
                                    responseAddress)
                                .ConfigureAwait(false);
                        }
                    })
                    .TransitionTo(OrderCreated),

                When(OrderCreatingAlreadySoldErrorEvent)
                    .ThenAsync(async context =>
                    {
                        logger.LogDebug("OrderCreatingAlreadyErrorEvent\r\n" +
                                        $"CorrelationId: {context.Data.CorrelationId}");

                        var (requestId, responseAddress) = context.Instance.GetCreateOrderRequestIdentity();

                        if (requestId.HasValue)
                        {
                            await RespondFromSaga(
                                    context,
                                    new CommandResponse<EmptyResult>(
                                        (int) OrderErrorCodes.AlreadySold,
                                        "Product already sold"),
                                    requestId.Value,
                                    responseAddress)
                                .ConfigureAwait(false);
                        }

                        //Do some compensate logic
                    })
                    .TransitionTo(OrderCreationError),

                When(OrderCreatingUnknownErrorEvent)
                    .ThenAsync(async context =>
                    {
                        logger.LogDebug("OrderCreatingUnknownErrorEvent\r\n" +
                                        $"CorrelationId: {context.Data.CorrelationId}");

                        var (requestId, responseAddress) = context.Instance.GetCreateOrderRequestIdentity();

                        if (requestId.HasValue)
                        {
                            await RespondFromSaga(
                                    context,
                                    new CommandResponse<EmptyResult>(
                                        (int) OrderErrorCodes.UnknownError,
                                        "Unknown order creating error"),
                                    requestId.Value,
                                    responseAddress)
                                .ConfigureAwait(false);
                        }

                        //Do some compensate logic
                    })
                    .TransitionTo(OrderCreationError));

            During(OrderCreated,
                When(AddOrderContacts)
                    .ThenAsync(async context =>
                    {
                        if (context.TryGetPayload(out ConsumeContext<AddOrderContactsCommand> consumeContext))
                        {
                            logger.LogDebug($"Receive command: {consumeContext.Message.GetType()}\r\n" +
                                            $"RequestId: {consumeContext.RequestId}\r\n" +
                                            $"CorrelationId: {consumeContext.Message.CorrelationId}");
                        }

                        await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);

                        context.Instance.Name = context.Data.Name;
                        context.Instance.Email = context.Data.Email;
                        context.Instance.Phone = context.Data.Phone;
                    })
                    .Respond(new CommandResponse<EmptyResult>())
                    .TransitionTo(CustomerContactsRecieved));

            During(CustomerContactsRecieved,
                When(PayOrder)
                    .ThenAsync(async context =>
                    {
                        if (context.TryGetPayload(out ConsumeContext<PayOrderCommand> consumeContext))
                        {
                            logger.LogDebug($"Receive command: {consumeContext.Message.GetType()}\r\n" +
                                            $"RequestId: {consumeContext.RequestId}\r\n" +
                                            $"CorrelationId: {consumeContext.Message.CorrelationId}");
                        }

                        var result = await commandProcessor.ProcessAsync(context.Data).ConfigureAwait(false);
                        await context.RespondAsync(new CommandResponse<PayOrderResult>(result)).ConfigureAwait(false);
                    }),
                When(OrderHasBeenPayedEvent)
                    .ThenAsync(async context =>
                    {
                        logger.LogDebug("OrderHasBeenPayedEvent\r\n" +
                                        $"CorrelationId: {context.Data.CorrelationId}");

                        await mailService.SendEmailOrder(
                            context.Instance.Name,
                            context.Instance.Email,
                            context.Instance.Phone).ConfigureAwait(false);
                    })
                    .TransitionTo(OrderHasBeenPayed));
        }

        private static async Task RespondFromSaga<TInstance, TEvent>(
            BehaviorContext<TInstance, TEvent> context,
            object response,
            Guid requestId,
            string responseAddress)
        {
            var sendEndpoint = await context.GetSendEndpoint(new Uri(responseAddress))
                .ConfigureAwait(false);

            await sendEndpoint.Send(response,
                sendContext =>
                {
                    sendContext.RequestId = requestId;
                }).ConfigureAwait(false);
        }
    }
}