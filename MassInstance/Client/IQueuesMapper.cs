using System;

namespace MassInstance.Client
{
    public interface IQueuesMapper
    {
        IQueuesMapper Map<TCommandType>(string queueName);

        string GetQueueName(Type commandType);

        string GetQueueName<TCommandType>();
    }
}