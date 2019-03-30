namespace MassInstance.Cqrs.Commands
{
    public interface IResultingCommand<out TResult> : ICommand
    {
    }
}