using System;

namespace Rds.CaraBus.RequestResponse
{
    public class Fault<TRequest> where TRequest : class
    {
        public Fault(TRequest request, Exception exception)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public TRequest Request { get;}

        public Exception Exception { get; }
    }
}