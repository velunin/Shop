using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using MassInstance.Client;
using MassInstance.Cqrs.Queries;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IMapper _mapper;

        public OrderController(
            IQueryService queryService, 
            IMapper mapper,
            IServiceClient serviceClient)
        {
            _queryService = queryService;
            _mapper = mapper;
            _serviceClient = serviceClient;
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

                try
                {
                    await _serviceClient.ProcessAsync(
                            new CreateOrderCommand(
                                orderId,
                                orderItems),
                            cancellationToken);
                }
                catch (ServiceException ex)
                {
                    throw new Exception($"{ex.ErrorCode} - {ex.Message}");
                }

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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _serviceClient
                .ProcessAsync(
                    new AddOrderContactsCommand(
                        model.OrderId,
                        model.Name,
                        model.Email,
                        model.Phone),
                    cancellationToken);

            return RedirectToAction("Payment", new
            {
                model.OrderId
            });
        }

        public IActionResult Payment(Guid orderId)
        {
            return View(new PaymentModel { OrderId = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> Payment(PaymentModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _serviceClient.ProcessAsync<PayOrderCommand, PayOrderResult>(
                new PayOrderCommand(model.OrderId),
                cancellationToken);

            model.Message = result.Message;

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

            return new CheckoutViewModel
            {
                CartItems = _mapper.Map<IEnumerable<CartItemModel>>(cartItems)
            };
        }
    }
}