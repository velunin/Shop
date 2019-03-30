using System;
using System.Linq;
using Shop.Cqrs.Commands;
using Shop.Infrastructure.Messaging;
using Shop.Services.Common.MessageContracts;

namespace Shop.Infrastructure.Configuration
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