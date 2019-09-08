using System;

namespace MassInstance.Configuration
{
    public class CommandConfiguration : ICommandConfiguration
    {
        public Action<CommandExceptionHandlingOptions> ConfigureExceptionHandling { get; private set; }

        public void SetExceptionHandling(Action<CommandExceptionHandlingOptions> configureExceptionHandling)
        {
            ConfigureExceptionHandling = configureExceptionHandling;
        }
    }
}