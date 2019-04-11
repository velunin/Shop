using System;

namespace MassInstance.Configuration
{
    public class CommandConfiguration : ICommandConfiguration
    {
        public Action<CommandExceptionHandlingOptions> ConfigureExceptionHandling { get; set; }
    }
}