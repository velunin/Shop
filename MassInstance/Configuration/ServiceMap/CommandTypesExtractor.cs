using System;
using System.Collections.Generic;
using System.Linq;

namespace MassInstance.Configuration.ServiceMap
{
    public class CommandTypesExtractor
    {
        private readonly List<Type> _services = new List<Type>();

        public CommandTypesExtractor From<TService>() where TService : IServiceMap
        {
            _services.Add(typeof(TService));

            return this;
        }

        public IEnumerable<Type> Extract()
        {
            return _services
                .Select(ServiceMapHelper.ExtractAllServiceCommands)
                .SelectMany(x => x.Select(t => t.Type))
                .Distinct();
        }
    }
}