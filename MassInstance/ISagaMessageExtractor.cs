using System;
using System.Collections.Generic;
using System.Reflection;

namespace MassInstance
{
    public interface ISagaMessageExtractor
    {
        IEnumerable<Type> Extract(Type sagaInstanceType, Assembly[] fromAssemblies);
    }
}