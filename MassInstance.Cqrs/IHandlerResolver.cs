using System;
using System.Collections.Generic;

namespace Shop.Cqrs
{
    public interface IHandlerResolver
    {
        object ResolveHandler(Type type);

        IEnumerable<object> ResolveHandlers(Type type);
    }
}