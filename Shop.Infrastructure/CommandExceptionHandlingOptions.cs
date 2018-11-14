using System;
using System.Collections.Generic;

namespace Shop.Infrastructure
{
    public class CommandExceptionHandlingOptions
    {
        public int DefaultErrorCode { get; private set; } = 0;

        public string DefaultErrorMessage { get; private set; } = "Unhandled exception";

        public void SetDefaultExceptionResponse(int code, string message)
        {
            DefaultErrorCode = code;
            DefaultErrorMessage = message;
        }

        public IDictionary<Type, Func<Exception, ExceptionResponse>> ExceptionMappings { get; }

        public CommandExceptionHandlingOptions()
        {
            ExceptionMappings = new Dictionary<Type, Func<Exception, ExceptionResponse>>();
        }

        public void Map<TException>(Func<TException, ExceptionResponse> func) where TException : Exception
        {
            if (func == null)
            {
               throw new ArgumentNullException(nameof(func));
            }

            var exceptionType = typeof(TException);
            var convertedFunc = ConvertFunc<TException, Exception, ExceptionResponse>(func);

            if (!ExceptionMappings.Keys.Contains(exceptionType))
            {
                ExceptionMappings.Add(exceptionType, ConvertFunc<TException, Exception, ExceptionResponse>(func));
            }
            else
            {
                ExceptionMappings[exceptionType] = convertedFunc;
            }
        }

        public void Map<TException>(int errorCode, string errorMessage) where TException : Exception
        {
            Map<TException>(exception => new ExceptionResponse{ Code = errorCode, Message = errorMessage});
        }

        public ExceptionResponse GetResponse(Exception ex)
        {
            if (ExceptionMappings.TryGetValue(ex.GetType(), out var func))
            {
                return func(ex);
            }

            if(ExceptionMappings.TryGetValue(typeof(Exception), out var defaultExceptionFunc))
            {
                return defaultExceptionFunc(ex);
            }

            return new ExceptionResponse
            {
                Code = DefaultErrorCode,
                Message = DefaultErrorMessage
            };
        }

        private static Func<TOut, TR> ConvertFunc<TIn, TOut, TR>(Func<TIn, TR> func) where TIn : TOut
        {
            return p => func((TIn)p);
        }
    }

    public class ExceptionResponse
    {
        public int Code { get; set; }

        public string Message { get; set; }
    }

}