using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MassInstance.Cqrs.Commands;
using MassInstance.Cqrs.Events;
using MassInstance.MessageContracts;

namespace MassInstance.Configuration.ServiceMap
{
    public class ServiceMapHelper
    {
        public static string ExtractQueueName(MemberInfo fieldInfo)
        {
            return ConvertQueueName(fieldInfo.Name);
        }

        public static string ConvertQueueName(string queueOriginalName)
        {
            return Regex.Replace(
                    queueOriginalName,
                    "([A-Z])", "-$0",
                    RegexOptions.Compiled)
                .Trim('-')
                .ToLower();
        }

        public static IEnumerable<QueueInfo> ExtractQueues(Type serviceMapType)
        {
             return serviceMapType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(IQueueMap)))
                .Select(f => new QueueInfo
                {
                    Name = ExtractQueueName(f),
                    Type = f.FieldType
                });
        }

        public static IEnumerable<CommandInfo> ExtractCommands(Type queueMapType)
        {
            return queueMapType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(ICommand)))
                .Select(f => new CommandInfo
                {
                    Name = f.Name,
                    Type = f.FieldType
                });
        }

        public static IEnumerable<EventInfo> ExtractEvents(Type queueMapType)
        {
            return queueMapType
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => f
                    .FieldType
                    .GetInterfaces()
                    .Any(i => i == typeof(IEvent)))
                .Select(f => new EventInfo
                {
                    Name = f.Name,
                    Type = f.FieldType
                });
        }

        public static IEnumerable<Type> ExtractCommandResultTypes(Type queueMapType)
        {
            var commandInfos = ExtractCommands(queueMapType);

            foreach (var commandInfo in commandInfos)
            {
                yield return GetCommandResultType(commandInfo.Type);
            }
        }

        public static IEnumerable<CommandInfo> ExtractServiceCommands(Type serviceMapType)
        {
            return ExtractQueues(serviceMapType).SelectMany(q => ExtractCommands(q.Type));
        }

        public static IEnumerable<Type> ExtractServiceCommandsResults(Type serviceMapType)
        {
            return ExtractQueues(serviceMapType).SelectMany(q => ExtractCommandResultTypes(q.Type));
        }

        private static Type GetCommandResultType(Type commandType)
        {
            var resultType = commandType.GetInterfaces()
                .Where(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResultingCommand<>))
                .Select(i => i.GetGenericArguments().FirstOrDefault())
                .FirstOrDefault();

            return resultType != null
                ? resultType
                : typeof(EmptyResult);
        }
    }
}