namespace Shop.Infrastructure.Messaging.MessageContracts
{
    public class CommandResponse<TResult>
    {
        public CommandExecutionStatus Status =>
            ErrorCode.HasValue ? CommandExecutionStatus.Error : CommandExecutionStatus.Ok;

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }


        public CommandResponse(int errorCode, string errorMessage)
        {
            Result = default(TResult);
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public CommandResponse()
        {
            
        }

        public CommandResponse(TResult result)
        {
            Result = result;
        }

        public TResult Result { get; }
    }

    public class EmptyResult
    {
    }

    public enum CommandExecutionStatus
    {
        Ok,
        Error
    }
}