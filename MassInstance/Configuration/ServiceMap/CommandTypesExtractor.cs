using System;
using System.Collections.Generic;
using System.Linq;

namespace MassInstance.Configuration.ServiceMap
{
    public class CommandTypesExtractor : ICommandTypesExtractor
    {
        private readonly List<Type> _consumerServices = new List<Type>();
        private readonly List<Type> _publishServices = new List<Type>();

        public ICommandTypesExtractor ConsumersFrom<TService>() where TService : IServiceMap
        {
            _consumerServices.Add(typeof(TService));

            return this;
        }

        public ICommandTypesExtractor ResultTypesFrom<TService>() where TService : IServiceMap
        {
            _publishServices.Add(typeof(TService));
            return this;
        }

        public IEnumerable<Type> ExtractCommands()
        {
            return _consumerServices
                .Select(ServiceMapHelper.ExtractServiceCommands)
                .SelectMany(x => x.Select(t => t.Type))
                .Distinct();
        }

        public IEnumerable<Type> ExtractResultTypes()
        {
            return _consumerServices
                .Select(ServiceMapHelper.ExtractServiceCommandsResults)
                .SelectMany(x => x)
                .Distinct();
        }
    }
}