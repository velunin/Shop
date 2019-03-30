using System;
using System.Collections.Generic;

namespace MassInstance.Cqrs
{
    public interface IHandlerResolver
    {
        object ResolveHandler(Type type);

        IEnumerable<object> ResolveHandlers(Type type);
    }
}