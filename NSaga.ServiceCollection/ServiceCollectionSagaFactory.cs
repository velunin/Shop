using System;

namespace NSaga.ServiceCollection
{
    public sealed class ServiceCollectionSagaFactory : ISagaFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceCollectionSagaFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public T ResolveSaga<T>() where T : class, IAccessibleSaga
        {
            return (T) _serviceProvider.GetService(typeof(T));
        }

        public IAccessibleSaga ResolveSaga(Type type)
        {
            return (IAccessibleSaga) _serviceProvider.GetService(type);
        }

        public IAccessibleSaga ResolveSagaInititatedBy(IInitiatingSagaMessage message)
        {
            var interfaceType = typeof(InitiatedBy<>).MakeGenericType(message.GetType());
            return (IAccessibleSaga) _serviceProvider.GetService(interfaceType);
        }

        public IAccessibleSaga ResolveSagaConsumedBy(ISagaMessage message)
        {
            var interfaceType = typeof(ConsumerOf<>).MakeGenericType(message.GetType());
            return (IAccessibleSaga) _serviceProvider.GetService(interfaceType);
        }
    }
}