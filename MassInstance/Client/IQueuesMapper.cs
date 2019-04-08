using System;

namespace MassInstance.Client
{
    public interface IQueuesMapper
    {
        IQueuesMapper Map<TCommandType>(string queueName);

        IQueuesMapper Map(Type commandType, string queueName);

        string GetQueueName(Type commandType);

        string GetQueueName<TCommandType>();
    }
}