﻿using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace MassInstance
{
    public class ServiceBusBackgroundService : IHostedService
    {
        private readonly IBusControl _serviceBus;

        public ServiceBusBackgroundService(
            IBusControl serviceBus)
        {
            _serviceBus = serviceBus;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _serviceBus.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _serviceBus.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}