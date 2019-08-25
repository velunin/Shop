using System.Threading.Tasks;

namespace MassInstance.Client
{
    public static class TaskExtensions
    {
        public static void DoNotAwait(this Task task)
        {
        }
    }
}