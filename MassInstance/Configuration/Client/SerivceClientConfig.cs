using System;

namespace MassInstance.Configuration.Client
{
    public class SerivceClientConfig
    {
        public Uri BrokerUri { get; set; }

        public string CallbackQueue { get; set; }
    }
}