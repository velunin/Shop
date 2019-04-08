using MassInstance.Configuration.ServiceMap;

namespace MassInstance.Client
{
    public interface IQueuesMapperBuilder
    {
        IQueuesMapperBuilder Add<TService>() where TService : IServiceMap;

        IQueuesMapper Build();
    }
}