using System;
using System.Linq;
using MassInstance.Cqrs.Commands;
using MassInstance.MessageContracts;
using MassInstance.Messaging;

namespace MassInstance.Configuration
{
    public class ConfigurationHelper
    {
        public static Type CreateConsumerTypeByCommandType(Type commandType)
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