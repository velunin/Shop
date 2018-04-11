using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rds.CaraBus.RequestResponse
{
    public interface IRequestClient<in TRequest,TResponse> 
    {
        Task<TResponse> Request(TRequest request, CancellationToken cancellationToken);

        Task<TResponse> Request(TRequest request, TimeSpan requestTimeout, CancellationToken cancellationToken);
    }
}
