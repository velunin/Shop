namespace MassInstance.Bus
{
    internal interface IRequestHandleControl
    {
        void SetResponse(object response);

        void SetCancelled();
    }
}