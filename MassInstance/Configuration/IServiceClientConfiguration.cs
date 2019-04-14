using System;
using MassInstance.Client;

namespace MassInstance.Configuration
{
    public interface IMassInstanceServiceClientConfigurator
    {
        void AddServiceClient(string callbackQueue, Action<IQueuesMapperBuilder> configureServices);
    }
}