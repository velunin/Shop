using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

using RDS.CaraBus;

using Shop.Infrastructure;

namespace Shop.Web.BackgroundServices
{
    public class ServiceBusBackgroundService : IHostedService
    {
        private readonly ICaraBus _caraBus;
        private readonly IAppServiceBusBootstrap _appServiceBusBootstrap;

        public ServiceBusBackgroundService(
            ICaraBus caraBus, IAppServiceBusBootstrap appServiceBusBootstrap)
        {
            _caraBus = caraBus;
            _appServiceBusBootstrap = appServiceBusBootstrap;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appServiceBusBootstrap.Startup(cancellationToken);

            return Task.CompletedTask;
        }

       
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _caraBus.StopAsync();
        }
    }
}