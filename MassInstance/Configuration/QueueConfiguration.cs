using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Configuration
{
    public class QueueConfiguration<TQueue> : IQueueConfiguration<TQueue> where TQueue : IQueueMap
    {
        private readonly IDictionary<Type, ICommandConfiguration> _commandConfigurations =
            new Dictionary<Type, ICommandConfiguration>();

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

        public ICommandConfiguration GetConfigurationForCommand(Type commandType)
        {
            return _commandConfigurations.TryGetValue(commandType, out var commandConfiguration) 
                ? commandConfiguration 
                : null;
        }

        public Action<CommandExceptionHandlingOptions> ConfigureCommandExceptionHandling { get; set; }

        private void Configure(Type commandType, Action<ICommandConfiguration> configureCommand = null)
        {
            var commandConfiguration = new CommandConfiguration();
            configureCommand?.Invoke(commandConfiguration);
            commandConfiguration.ConfigureExceptionHandling =
                ConfigureCommandExceptionHandling +
                commandConfiguration.ConfigureExceptionHandling;

            _commandConfigurations.Add(commandType, commandConfiguration);
        }
    }
}