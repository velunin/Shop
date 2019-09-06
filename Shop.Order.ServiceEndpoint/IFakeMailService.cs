using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Shop.Order.ServiceEndpoint
{
    public interface IFakeMailService
    {
        Task SendEmailOrder(string name, string email, string phone);
    }

    public class FakeMailService : IFakeMailService
    {
        private readonly ILogger _logger;

        public FakeMailService(ILogger<FakeMailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailOrder(string name, string email, string phone)
        {
            _logger.LogInformation($"Send email to {email}");

            return Task.CompletedTask;
        }
    }
}