using System;
using System.Collections.Generic;

namespace MassInstance.Configuration.ServiceMap
{
    public interface ICommandTypesExtractor
    {
        ICommandTypesExtractor ConsumersFrom<TService>() where TService : IServiceMap;

        ICommandTypesExtractor ResultTypesFrom<TService>() where TService : IServiceMap;

        IEnumerable<Type> ExtractCommands();

        IEnumerable<Type> ExtractResultTypes();
    }
}