using System;
using System.Threading;
using System.Threading.Tasks;

using GreenPipes;
using MassInstance.Cqrs.Events;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Topology;

namespace MassInstance.Bus
{
    public partial class ServiceBus : IServiceBus
    {
        private readonly ILog _log = Logger.Get<ServiceBus>();
        private readonly IBusControl _busControl;

        public ServiceBus(IBusControl busControl)
        {
            _busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
        }

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            return _busControl.ConnectPublishObserver(observer);
        }

        public Task Publish<T>(T message, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return _busControl.Publish(message, cancellationToken);
        }

        public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return _busControl.Publish(message, publishPipe, cancellationToken);
        }

        public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return _busControl.Publish(message, publishPipe, cancellationToken);
        }

        public Task Publish(object message, CancellationToken cancellationToken = new CancellationToken())
        {
            return _busControl.Publish(message, cancellationToken);
        }

        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken())
        {
            return _busControl.Publish(message, publishPipe, cancellationToken);
        }

        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = new CancellationToken())
        {
            return _busControl.Publish(message, messageType, cancellationToken);
        }

        public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _busControl.Publish(message, messageType, publishPipe, cancellationToken);
        }

        public Task Publish<T>(object values, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return _busControl.Publish(values, cancellationToken);
        }

        public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return _busControl.Publish(values, publishPipe, cancellationToken);
        }

        public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return _busControl.Publish(values, cancellationToken);
        }

        public ConnectHandle ConnectSendObserver(ISendObserver observer)
        {
            return _busControl.ConnectSendObserver(observer);
        }

        public Task<ISendEndpoint> GetSendEndpoint(Uri address)
        {
            return _busControl.GetSendEndpoint(address);
        }

        public ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe) where T : class
        {
            return _busControl.ConnectConsumePipe(pipe);
        }

        public ConnectHandle ConnectRequestPipe<T>(Guid requestId, IPipe<ConsumeContext<T>> pipe) where T : class
        {
            return _busControl.ConnectRequestPipe(requestId, pipe);
        }

        public ConnectHandle ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer) where T : class
        {
            return _busControl.ConnectConsumeMessageObserver(observer);
        }

        public ConnectHandle ConnectConsumeObserver(IConsumeObserver observer)
        {
            return _busControl.ConnectConsumeObserver(observer);
        }

        public ConnectHandle ConnectReceiveObserver(IReceiveObserver observer)
        {
            return _busControl.ConnectReceiveObserver(observer);
        }

        public ConnectHandle ConnectReceiveEndpointObserver(IReceiveEndpointObserver observer)
        {
            return _busControl.ConnectReceiveEndpointObserver(observer);
        }

        public void Probe(ProbeContext context)
        {
            _busControl.Probe(context);
        }

        public Uri Address => _busControl.Address;

        public IBusTopology Topology => _busControl.Topology;

        public Task<BusHandle> StartAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _busControl.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _busControl.StopAsync(cancellationToken);
        }
    }
}