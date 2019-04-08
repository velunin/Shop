using System;

namespace MassInstance
{
    public interface IExceptionResponseResolver
    {
        void Map(Type commandType, CommandExceptionHandlingOptions options);

        bool TryResolveResponse(Type commandType, Exception ex, out ExceptionResponse response);
    }
}