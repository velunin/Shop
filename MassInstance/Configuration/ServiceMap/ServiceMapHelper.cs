using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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
                    .Any(i => i == typeof(IQueueMap)))
                .Select(f => new CommandInfo
                {
                    Name = f.Name,
                    Type = f.FieldType
                });
        }

        public static IEnumerable<CommandInfo> ExtractAllServiceCommands(Type serviceMapType)
        {
            return ExtractQueues(serviceMapType).SelectMany(q => ExtractCommands(q.Type));
        }
    }
}