using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;

namespace Shop.Infrastructure.Messaging
{
    public class CommandConsumerRescueContextFactory
    {
        public static CommandConsumerRescueContext<TConsumer> Create<TConsumer>(
            ConsumerConsumeContext<TConsumer> context, 
            Exception exception) where TConsumer : class
        {
            return new CommandConsumerRescueContext<TConsumer>(context, exception);
        }
    }

    public class CommandConsumerRescueContext<TConsumer> :
        BasePipeContext,
        ConsumerConsumeContext<TConsumer>
        where TConsumer : class
    {
        public CommandConsumerRescueContext(ConsumerConsumeContext<TConsumer> context, Exception exception)
        {
            Context = context;
            Exception = exception;
        }

        public Exception Exception { get; }

        public ConsumerConsumeContext<TConsumer> Context { get; }

        public Guid? MessageId => Context.MessageId;

        public Guid? RequestId => Context.RequestId;

        public Guid? CorrelationId => Context.CorrelationId;

        public Guid? ConversationId => Context.ConversationId;

        public Guid? InitiatorId => Context.InitiatorId;

        public DateTime? ExpirationTime => Context.ExpirationTime;

        public Uri SourceAddress => Context.SourceAddress;

        public Uri DestinationAddress => Context.DestinationAddress;

        public Uri ResponseAddress => Context.ResponseAddress;

        public Uri FaultAddress => Context.FaultAddress;

        public DateTime? SentTime => Context.SentTime;

        public Headers Headers => Context.Headers;

        public HostInfo Host => Context.Host;

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            return Context.ConnectPublishObserver(observer);
        }

        public Task Publish<T>(T message, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return Context.Publish(message, cancellationToken);
        }

        public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return Context.Publish(message, publishPipe, cancellationToken);
        }

        public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return Context.Publish(message, publishPipe, cancellationToken);
        }

        public Task Publish(object message, CancellationToken cancellationToken = new CancellationToken())
        {
            return Context.Publish(message, cancellationToken);
        }

        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken())
        {
            return Context.Publish(message, publishPipe, cancellationToken);
        }

        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = new CancellationToken())
        {
            return Context.Publish(message, messageType, cancellationToken);
        }

        public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Context.Publish(message, messageType, publishPipe, cancellationToken);
        }

        public Task Publish<T>(object values, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return Context.Publish<T>(values, cancellationToken);
        }

        public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return Context.Publish(values, publishPipe, cancellationToken);
        }

        public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return Context.Publish<T>(values, publishPipe, cancellationToken);
        }

        public ConnectHandle ConnectSendObserver(ISendObserver observer)
        {
            return Context.ConnectSendObserver(observer);
        }

        public Task<ISendEndpoint> GetSendEndpoint(Uri address)
        {
            return Context.GetSendEndpoint(address);
        }

        public bool HasMessageType(Type messageType)
        {
            return Context.HasMessageType(messageType);
        }

        public bool TryGetMessage<T>(out ConsumeContext<T> consumeContext) where T : class
        {
            return Context.TryGetMessage(out consumeContext);
        }

        public Task RespondAsync<T>(T message) where T : class
        {
            return Context.RespondAsync(message);
        }

        public Task RespondAsync<T>(T message, IPipe<SendContext<T>> sendPipe) where T : class
        {
            return Context.RespondAsync(message, sendPipe);
        }

        public Task RespondAsync<T>(T message, IPipe<SendContext> sendPipe) where T : class
        {
            return Context.RespondAsync(message, sendPipe);
        }

        public Task RespondAsync(object message)
        {
            return Context.RespondAsync(message);
        }

        public Task RespondAsync(object message, Type messageType)
        {
            return Context.RespondAsync(message, messageType);
        }

        public Task RespondAsync(object message, IPipe<SendContext> sendPipe)
        {
            return Context.RespondAsync(message, sendPipe);
        }

        public Task RespondAsync(object message, Type messageType, IPipe<SendContext> sendPipe)
        {
            return Context.RespondAsync(message, messageType, sendPipe);
        }

        public Task RespondAsync<T>(object values) where T : class
        {
            return Context.RespondAsync<T>(values);
        }

        public Task RespondAsync<T>(object values, IPipe<SendContext<T>> sendPipe) where T : class
        {
            return Context.RespondAsync(values, sendPipe);
        }

        public Task RespondAsync<T>(object values, IPipe<SendContext> sendPipe) where T : class
        {
            return Context.RespondAsync<T>(values, sendPipe);
        }

        public void Respond<T>(T message) where T : class
        {
            Context.Respond(message);
        }

        public Task NotifyConsumed<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
        {
            return Context.NotifyConsumed(context, duration, consumerType);
        }

        public Task NotifyFaulted<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
        {
            return Context.NotifyFaulted(context, duration, consumerType, exception);
        }

        public ReceiveContext ReceiveContext => Context.ReceiveContext;

        public Task CompleteTask => Context.CompleteTask;

        public IEnumerable<string> SupportedMessageTypes => Context.SupportedMessageTypes;

        public ConsumerConsumeContext<TConsumer, T> PopContext<T>() where T : class
        {
            return Context.PopContext<T>();
        }

        public TConsumer Consumer => Context.Consumer;
    }
}