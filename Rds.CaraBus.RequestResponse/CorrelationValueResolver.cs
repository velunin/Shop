using System;
using System.Collections.Concurrent;

namespace Rds.CaraBus.RequestResponse
{
    internal class CorrelationValueResolver : ICorrelationValueResolver
    {
        private readonly ConcurrentDictionary<Type, Func<object, Guid>> _correlationIdExtractors =
            new ConcurrentDictionary<Type, Func<object, Guid>>();

        public bool IsCorrelate<TRequest, TResponse>(TRequest request, TResponse response)
        {
            var requestType = typeof(TRequest);
            var responseType = typeof(TResponse);

            if (!_correlationIdExtractors.TryGetValue(requestType, out var requestResolver))
            {
                ThrowCorrelationFindingException(requestType);
            }

            if (!_correlationIdExtractors.TryGetValue(responseType, out var responseResolver))
            {
                ThrowCorrelationFindingException(responseType);
            }

            // ReSharper disable once PossibleNullReferenceException
            var requestCorrelationId = requestResolver.Invoke(request);
            // ReSharper disable once PossibleNullReferenceException
            var responseCorrelationId = responseResolver.Invoke(response);

            return requestCorrelationId == responseCorrelationId;
        }

        public void UseCorrelationId<T>(Func<T, Guid> correlationIdExtractor)
        {
            UseCorrelationId(typeof(T), ConvertFunc<T, object, Guid>(correlationIdExtractor));
        }

        public void UseCorrelationId(Type type, Func<object, Guid> correlationIdExtractor)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (correlationIdExtractor == null)
            {
                throw new ArgumentNullException(nameof(correlationIdExtractor));
            }

            _correlationIdExtractors.TryAdd(type, correlationIdExtractor);
        }

        private static Func<TOut, TR> ConvertFunc<TIn, TOut, TR>(Func<TIn, TR> func) where TIn : TOut
        {
            return p => func((TIn)p);
        }

        private static void ThrowCorrelationFindingException(Type messageType)
        {
            throw new InvalidOperationException($"Correlation Id extractor for {messageType.FullName} not found");
        }

    }
}