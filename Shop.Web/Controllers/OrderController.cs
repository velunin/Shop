using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Rds.Cqrs.Commands;
using Rds.Cqrs.Queries;

using Shop.DataProjections.Queries;
using Shop.Domain.Commands.Order;
using Shop.Domain.Commands.Order.Results;
using Shop.Services.Common;
using Shop.Web.Models;

namespace Shop.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IQueryService _queryService;
        private readonly IServiceClient _serviceClient;
        private readonly ICommandProcessor _commandProcessor;
        private readonly IMapper _mapper;

        public OrderController(
            IQueryService queryService, 
            IMapper mapper,
            IServiceClient serviceClient, 
            ICommandProcessor commandProcessor)
        {
            _queryService = queryService;
            _mapper = mapper;
            _serviceClient = serviceClient;
            _commandProcessor = commandProcessor;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(await GetCheckoutViewModel(cancellationToken));
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(CancellationToken cancellationToken)
        {
            try
            {
                var orderItems = await GetOrderItems(cancellationToken);

                var orderId = Guid.NewGuid();

                await _commandProcessor.ProcessAsync(
                    new CreateOrderCommand(
                        orderId,
                        orderItems),
                    cancellationToken);

                #region OverServiceBus
                try
                {
                    //await _serviceClient.ProcessAsync(
                    //        new CreateOrderCommand(
                    //            orderId,
                    //            orderItems),
                    //        TimeSpan.FromSeconds(5),
                    //        cancellationToken);
                }
                catch (ServiceException ex)
                {
                    throw new Exception($"{ex.ErrorCode} - {ex.Message}");
                }
                #endregion

                return RedirectToAction("Contacts", new
                {
                    orderId
                });
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;

                return View("Index", 
                    await GetCheckoutViewModel(cancellationToken));
            }
        }

        public IActionResult Contacts(Guid orderId, CancellationToken cancellationToken)
        {
            return View(new AddOrderContactModel { OrderId = orderId });
        }
        
        [HttpPost]
        public async Task<IActionResult> Contacts(AddOrderContactModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {

                await _commandProcessor.ProcessAsync(
                    new AddOrderContactsCommand(
                        model.OrderId,
                        model.Name,
                        model.Email,
                        model.Phone),
                    cancellationToken);

                //await _serviceClient
                //    .ProcessAsync(
                //        new AddOrderContactsCommand(
                //            model.OrderId,
                //            model.Name,
                //            model.Email,
                //            model.Phone),
                //        cancellationToken);

                return RedirectToAction("Payment", new
                {
                    model.OrderId
                });
            }

            return View(model);
        }

        public IActionResult Payment(Guid orderId)
        {
            return View(new PaymentModel { OrderId = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> Payment(PaymentModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _commandProcessor.ProcessAsync(
                        new PayOrderCommand(model.OrderId),
                        cancellationToken);

                //   var result = await _serviceClient.ProcessAsync<PayOrderCommand, PayOrderResult>(
                //            new PayOrderCommand(model.OrderId),
                //            TimeSpan.FromSeconds(5),
                //            cancellationToken);

                model.Message = result.Message;
            }

            return View(model);
        }

        private async Task<OrderItem[]> GetOrderItems(CancellationToken cancellationToken)
        {
            var cartItems = await _queryService
                .QueryAsync(
                    new GetCartItems(HttpContext.Session.Id),
                    cancellationToken);

            return _mapper.Map<IEnumerable<OrderItem>>(cartItems).ToArray();
        }

        private async Task<CheckoutViewModel> GetCheckoutViewModel(CancellationToken cancellationToken)
        {
            var cartItems = await _queryService
                .QueryAsync(
                    new GetCartItems(HttpContext.Session.Id),
                    cancellationToken);

            var model = new CheckoutViewModel
            {
                CartItems = _mapper.Map<IEnumerable<CartItemModel>>(cartItems)
            };
            return model;
        }
    }
}