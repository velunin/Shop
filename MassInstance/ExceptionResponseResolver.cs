using System;
using System.Collections.Concurrent;

namespace MassInstance
{
    public class ExceptionResponseResolver : IExceptionResponseResolver
    {
        private readonly ConcurrentDictionary<Type, CommandExceptionHandlingOptions> _typeToOptionsMap =
            new ConcurrentDictionary<Type, CommandExceptionHandlingOptions>();

        public void Map(Type commandType, CommandExceptionHandlingOptions options)
        {
            _typeToOptionsMap.AddOrUpdate(
                commandType, 
                options, 
                (type, handlingOptions) => options);
        }

        public bool TryResolveResponse(Type commandType, Exception ex, out ExceptionResponse response)
        {
            if (_typeToOptionsMap.TryGetValue(commandType, out var commandExceptionHandlingOptions))
            {
                response = commandExceptionHandlingOptions.GetResponse(ex);
                return true;
            }

            response = null;
            return false;
        }
    }
}