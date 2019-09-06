using System;
using MassInstance;

namespace Shop.Order.ServiceEndpoint
{
    public class ExceptionResponseMappings
    {
        public static void DefaultOrderServiceMap(CommandExceptionHandlingOptions options)
        {
            options.SetDefaultExceptionResponse((int)OrderErrorCodes.UnknownError, "Unknown error");
        }

        public static void CreateOrderCommandMap(CommandExceptionHandlingOptions options)
        {
            options.Map<InvalidOperationException>(exp =>
            {
                var message = exp.Message;
                var code = 500; //get some data from exception

                return new ExceptionResponse
                {
                    Code = code,
                    Message = message
                };
            });
        }
    }
}