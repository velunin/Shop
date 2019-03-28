using System;
using System.Collections.Generic;
using System.Linq;
using Shop.Cqrs.Commands;
using Shop.Infrastructure.Messaging;

namespace Shop.Infrastructure
{
    public interface ICommandConsumerFactory
    {
        object CreateConsumer(Type commandType);

        IEnumerable<Type> GetConsumerTypes(T)
    }

    public class ServiceCollectionCommandConsumerFactory : ICommandConsumerFactory
    {
        public object CreateConsumer(Type commandType)
        {
            var commandResultTypes = commandType
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResultingCommand<>))
                .Select(i => i.GetGenericArguments().FirstOrDefault())
                .ToList();

            var consumerType = typeof(CommandRequestConsumer<,>).MakeGenericType(commandType)
            throw new NotImplementedException();
        }
    }
}