namespace MassInstance.Bus
{
    internal interface IRequestHandleResponseSetter
    {
        void SetResponse(object response);

        void SetCancelled();
    }
}