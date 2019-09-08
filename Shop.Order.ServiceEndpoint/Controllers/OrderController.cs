using System.Threading.Tasks;
using MassInstance.Cqrs.Commands;
using Microsoft.AspNetCore.Mvc;
using Shop.Order.Domain.Commands;
using Shop.Order.ServiceModels;

namespace Shop.Order.ServiceEndpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ICommandProcessor _commandProcessor;

        public OrderController(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrderContacts(AddOrderContactsModel addOrderContactsModel)
        {
            await _commandProcessor.ProcessAsync(
                new AddOrderContactsCommand(
                    addOrderContactsModel.OrderId,
                    addOrderContactsModel.Name,
                    addOrderContactsModel.Email,
                    addOrderContactsModel.Phone));

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Pay(PaymentModel paymentModel)
        {
            await _commandProcessor.ProcessAsync(new PayOrderCommand(paymentModel.OrderId));

            return Ok();
        }
    }
}