using System;

namespace Rds.CaraBus.RequestResponse
{
    public interface ICorrelationValueResolver
    {
        bool IsCorrelate<TRequest,TResponse>(TRequest request, TResponse response);

        void UseCorrelationId<T>(Func<T, Guid> correlationIdExtractor);

        void UseCorrelationId(Type type, Func<object, Guid> correlationIdExtractor);
    }
}