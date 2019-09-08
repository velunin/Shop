using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Automatonymous;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public class QueueConfiguration<TQueue> : IQueueConfiguration<TQueue> where TQueue : IQueueMap
    {
        private readonly IDictionary<Type, ICommandConfiguration> _commandConfigurations =
            new Dictionary<Type, ICommandConfiguration>();

        private readonly HashSet<Type> _sagaInstanceTypes = new HashSet<Type>();

        public ICommandConfiguration SelectCommand<TCommand>(
            Expression<Func<TQueue, TCommand>> commandSelector)
        {
            return CreateAndGetConfiguration(commandSelector.ReturnType);
        }

        public ICommandConfiguration SelectCommand<TQueueMap, TCommand>(
            Expression<Func<TQueueMap, TCommand>> commandSelector) where TQueueMap : IQueueMap
        {
            return CreateAndGetConfiguration(commandSelector.ReturnType);
        }

        public void ConfigureSaga<TSagaInstance>() where TSagaInstance : SagaStateMachineInstance
        {
            _sagaInstanceTypes.Add(typeof(TSagaInstance));
        }

        public ICommandConfiguration GetConfigurationForCommand(Type commandType)
        {
            return _commandConfigurations.TryGetValue(commandType, out var commandConfiguration) 
                ? commandConfiguration 
                : null;
        }

        public IEnumerable<Type> GetSagaInstanceTypes()
        {
            return _sagaInstanceTypes;
        }

        public bool TryGetCommandConfig(Type commandType, out ICommandConfiguration commandConfiguration)
        {
            return _commandConfigurations.TryGetValue(commandType, out commandConfiguration);
        }

        public Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        private ICommandConfiguration CreateAndGetConfiguration(Type commandType)
        {
            if (_commandConfigurations.ContainsKey(commandType))
            {
                return _commandConfigurations[commandType];
            }

            var commandConfiguration = new CommandConfiguration();

            _commandConfigurations.Add(commandType, commandConfiguration);

            return commandConfiguration;
        }
    }
}