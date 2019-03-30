using System.Threading;
using System.Threading.Tasks;

namespace Shop.Cqrs.Queries
{
    public interface IQueryService
    {
        Task<TResult> QueryAsync<TResult>(
            IQuery<TResult> query,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}