using System;

namespace Shop.Infrastructure
{
    public interface ICommandConsumerFactory
    {
        object CreateConsumer(Type messageType);
    }

    public class ServiceCollectionCommandConsumerFactory : ICommandConsumerFactory
    {
        public object CreateConsumer(Type messageType)
        {
            throw new NotImplementedException();
        }
    }
}