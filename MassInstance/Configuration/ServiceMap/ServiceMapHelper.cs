using System.Reflection;
using System.Text.RegularExpressions;

namespace MassInstance.Configuration.ServiceMap
{
    public class ServiceMapHelper
    {
        public static string ExtractQueueName(MemberInfo fieldInfo)
        {
            return Regex.Replace(
                    fieldInfo.Name,
                    "([A-Z])", "-$0",
                    RegexOptions.Compiled)
                .Trim('-')
                .ToLower();
        }
    }
}