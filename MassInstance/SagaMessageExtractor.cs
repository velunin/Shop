using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatonymous;

namespace MassInstance
{
    public class SagaMessageExtractor : ISagaMessageExtractor
    {
        public IEnumerable<Type> Extract(Type sagaInstanceType, Assembly[] fromAssemblies)
        {
            var sagaStateMachineInterfaceType = typeof(SagaStateMachine<>).MakeGenericType(sagaInstanceType);

            var sagaStateMachineType = fromAssemblies
                .SelectMany(s => s.GetTypes())
                .Single(p => sagaStateMachineInterfaceType.IsAssignableFrom(p));

            var eventStateMachineType = typeof(Event<>);

            return sagaStateMachineType
                .GetProperties()
                .Where(p => eventStateMachineType.IsAssignableFrom(p.PropertyType))
                .Select(p => p.PropertyType.GetGenericArguments().FirstOrDefault());
        }
    }
}