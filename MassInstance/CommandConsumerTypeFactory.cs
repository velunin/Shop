using System;
using System.Linq;
using MassInstance.Cqrs.Commands;
using MassInstance.MessageContracts;
using MassInstance.Messaging;

namespace MassInstance
{
    public class CommandConsumerTypeFactory
    {
        public static Type Create(Type commandType)
        {
            var resultType = commandType
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResultingCommand<>))
                .Select(i => i.GetGenericArguments().FirstOrDefault())
                .SingleOrDefault();

            return typeof(CommandRequestConsumer<,>).MakeGenericType(commandType, resultType ?? typeof(EmptyResult));
        }
    }
}