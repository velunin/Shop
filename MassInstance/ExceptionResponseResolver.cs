using System;
using System.Collections.Concurrent;

namespace MassInstance
{
    public class ExceptionResponseResolver// : IExceptionResponseResolver
    {
        private static readonly ConcurrentDictionary<Type, CommandExceptionHandlingOptions> TypeToOptionsMap =
            new ConcurrentDictionary<Type, CommandExceptionHandlingOptions>();

        public static void Map(Type commandType, CommandExceptionHandlingOptions options)
        {
            TypeToOptionsMap.AddOrUpdate(
                commandType, 
                options, 
                (type, handlingOptions) => options);
        }

        public static bool TryResolveResponse(Type commandType, Exception ex, out ExceptionResponse response)
        {
            if (TypeToOptionsMap.TryGetValue(commandType, out var commandExceptionHandlingOptions))
            {
                response = commandExceptionHandlingOptions.GetResponse(ex);
                return true;
            }

            response = null;
            return false;
        }
    }
}