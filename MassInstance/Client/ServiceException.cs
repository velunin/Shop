using System;

namespace MassInstance.Client
{
    public class ServiceException : Exception
    {
        public int ErrorCode { get; }

        public ServiceException(string message, int code) : base(message)
        {
            ErrorCode = code;
        }
        public ServiceException(string message, Exception inner, int code) : base(message, inner)
        {
            ErrorCode = code;
        }
    }
}