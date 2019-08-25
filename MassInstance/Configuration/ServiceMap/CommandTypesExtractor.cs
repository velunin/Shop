using System;
using System.Collections.Generic;

namespace MassInstance.Configuration.ServiceMap
{
    public class CommandTypesExtractor : ICommandTypesExtractor
    {
        private readonly HashSet<Type> _commandTypes = new HashSet<Type>();
        private readonly HashSet<Type> _eventTypes = new HashSet<Type>();
        private readonly HashSet<Type> _commandResultTypes = new HashSet<Type>();

        public ICommandTypesExtractor ConsumersFrom<TService>() where TService : IServiceMap
        {
            foreach (var commandInfo in ServiceMapHelper.ExtractServiceCommands(typeof(TService)))
            {
                if (!_commandTypes.Contains(commandInfo.Type))
                {
                    _commandTypes.Add(commandInfo.Type);
                }
            }

            foreach (var eventInfo in ServiceMapHelper.ExtractServiceEvents(typeof(TService)))
            {
                if (!_eventTypes.Contains(eventInfo.Type))
                {
                    _eventTypes.Add(eventInfo.Type);
                }
            }

            return this;
        }

        public ICommandTypesExtractor ResultTypesFrom<TService>() where TService : IServiceMap
        {

            foreach (var commandResultType in ServiceMapHelper.ExtractServiceCommandsResults(typeof(TService)))
            {
                if (!_commandResultTypes.Contains(commandResultType))
                {
                    _commandResultTypes.Add(commandResultType);
                }
            }

            return this;
        }

        public IEnumerable<Type> ExtractCommands()
        {
            return _commandTypes;
        }

        public IEnumerable<Type> ExtractResultTypes()
        {
            return _commandResultTypes;
        }

        public IEnumerable<Type> ExtractEventTypes()
        {
            return _eventTypes;
        }
    }
}