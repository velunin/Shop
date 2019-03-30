using System;

namespace Shop.Services.Common
{
    public interface IQueuesMapper
    {
        IQueuesMapper Map<TCommandType>(string queueName);

        string GetQueueName(Type commandType);

        string GetQueueName<TCommandType>();
    }
}