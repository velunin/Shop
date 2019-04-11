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

        public void Configure<TCommand>(
            Expression<Func<TQueue, TCommand>> commandSelector, 
            Action<ICommandConfiguration> configureCommand = null)
        {
            Configure(commandSelector.ReturnType, configureCommand);
        }

        public void Configure<TQueueMap, TCommand>(
            Expression<Func<TQueueMap, TCommand>> commandSelector, 
            Action<ICommandConfiguration> configureCommand = null) where TQueueMap : IQueueMap
        {
            Configure(commandSelector.ReturnType, configureCommand);
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

        private void Configure(Type commandType, Action<ICommandConfiguration> configureCommand = null)
        {
            if (_commandConfigurations.ContainsKey(commandType))
            {
                throw new InvalidOperationException($"Command {commandType} already configured");
            }

            var commandConfiguration = new CommandConfiguration();
            configureCommand?.Invoke(commandConfiguration);

            _commandConfigurations.Add(commandType, commandConfiguration);
        }
    }
}