using System;

namespace MassInstance.Configuration
{
    public interface ICommandConfiguration
    {
        Action<CommandExceptionHandlingOptions> ConfigureExceptionHandling { get; }

        void SetExceptionHandling(Action<CommandExceptionHandlingOptions> configureExceptionHandling);
    }
}