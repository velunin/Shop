using System;
using System.Linq;
using MassInstance.Cqrs.Commands;
using MassInstance.MessageContracts;
using MassInstance.Messaging;

namespace MassInstance
{
    public class CommandConsumerTypeFactory
    {
        public static Type CreateCommandConsumer(Type commandType)
        {
            var resultType = commandType
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResultingCommand<>))
                .Select(i => i.GetGenericArguments().FirstOrDefault())
                .SingleOrDefault();

            return typeof(CommandRequestConsumer<,>).MakeGenericType(commandType, resultType ?? typeof(EmptyResult));
        }

        public static Type CreateEventConsumer(Type eventType)
        {
            return typeof(EventConsumer<>).MakeGenericType(eventType);
        }

        public static Type CreateCallbackConsumer(Type commandResultType)
        {
            return typeof(CallbackCommandConsumer<,>).MakeGenericType(
                typeof(CommandResponse<>).MakeGenericType(commandResultType), commandResultType);
        }
    }
}