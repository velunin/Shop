namespace Shop.Cqrs.Commands
{
    public interface IResultingCommand<out TResult> : ICommand
    {
    }
}