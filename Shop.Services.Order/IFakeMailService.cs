using System.Threading.Tasks;

namespace Shop.Services.Order
{
    public interface IFakeMailService
    {
        Task SendEmailOrder(string name, string email, string phone);
    }

    public class FakeMailService : IFakeMailService
    {
        public Task SendEmailOrder(string name, string email, string phone)
        {
            return Task.CompletedTask;
        }
    }
}