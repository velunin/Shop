using System;

namespace MassInstance.Configuration
{
    public interface ICommandConfiguration
    {
        Action<CommandExceptionHandlingOptions> ConfigureExceptionHandling { get; set; }
    }

    public class CommandConfiguration : ICommandConfiguration
    {
        public Action<CommandExceptionHandlingOptions> ConfigureExceptionHandling { get; set; }
    }
}