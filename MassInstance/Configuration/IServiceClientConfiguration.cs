using System;
using MassInstance.Client;
using MassInstance.Configuration.Client;

namespace MassInstance.Configuration
{
    public interface IMassInstanceServiceClientConfigurator
    {
        void AddServiceClient(SerivceClientConfig config, Action<IQueuesMapperBuilder> configureMapperBuilder);
    }
}