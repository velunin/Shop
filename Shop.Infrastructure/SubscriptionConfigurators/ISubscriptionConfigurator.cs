using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shop.Infrastructure.SubscriptionConfigurators
{
    public interface ISubscriptionConfigurator
    {
        Task ConfigureAsync(IEnumerable<Type> sourceTypes, CancellationToken cancellationToken);
    }
}