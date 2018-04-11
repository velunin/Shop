using System.Threading;

namespace Shop.Infrastructure
{
    public interface IAppServiceBusBootstrap
    {
        void Startup(CancellationToken cancellationToken);
    }
}