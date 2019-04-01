using System;

namespace MassInstance
{
    public interface ICommandConsumerFactory
    {
        object CreateConsumer(Type commandType);
    }
}