using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Shop.Cqrs.Queries
{
    public class QueryService : IQueryService
    {
        private readonly IHandlerResolver _handlerResolver;

        private readonly ConcurrentDictionary<Type, (Type,Func<object, object, CancellationToken, Task>)>
            _queryHandlersInvokers = new ConcurrentDictionary<Type, (Type, Func<object, object, CancellationToken, Task>)>();

        public QueryService(IHandlerResolver handlerResolver)
        {
            _handlerResolver = handlerResolver;
        }

        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var queryType = query.GetType();

            var (queryHandlerType, handlerInvoker) = _queryHandlersInvokers.GetOrAdd(queryType,CreateHandlerInvoker<TResult>);

            var handlerInstance = _handlerResolver.ResolveHandler(queryHandlerType);

            return await ((Task<TResult>) handlerInvoker(handlerInstance, query, cancellationToken))
                .ConfigureAwait(false);
        }

        private static (Type, Func<object, object, CancellationToken, Task>) CreateHandlerInvoker<TResult>(Type queryType)
        {
            var queryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

            var handleMethod = queryHandlerType.GetMethod("HandleAsync", BindingFlags.Public | BindingFlags.Instance);

            var queryHandlerParameter = Expression.Parameter(typeof(object));
            var queryParameter = Expression.Parameter(typeof(object));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

            var queryHandlerVariable = Expression.Variable(queryHandlerType, "queryHandler");
            var queryVariable = Expression.Variable(queryType, "query");

            var block = Expression.Block(new[] {queryHandlerVariable, queryVariable},
                Expression.Assign(
                    queryHandlerVariable,
                    Expression.Convert(queryHandlerParameter, queryHandlerType)),
                Expression.Assign(
                    queryVariable,
                    Expression.Convert(queryParameter, queryType)),
                Expression.Call(
                    queryHandlerVariable, 
                    handleMethod,
                    queryVariable,
                    cancellationTokenParameter));

            var lambda = Expression.Lambda<Func<object, object, CancellationToken, Task>>(
                block,
                queryHandlerParameter,
                queryParameter,
                cancellationTokenParameter);

            return (queryHandlerType, lambda.Compile());
        }
    }
}