using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Rds.CaraBus.RequestResponse;
using Rds.Cqrs.Queries;
using Shop.DataProjections.Queries;
using Shop.Domain.Commands.Order;
using Shop.Domain.Events;
using Shop.Web.Models;

namespace Shop.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IQueryService _queryService;
        private readonly IRequestClient<CreateOrderMessage, OrderCreated> _createOrderClient;
        private readonly IRequestClient<AddOrderContactsMessage, OrderContactsAdded> _addContactsClient;
        private readonly IMapper _mapper;

        public OrderController(
            IQueryService queryService, 
            IMapper mapper,
            IRequestClient<CreateOrderMessage, OrderCreated> createOrderClient, 
            IRequestClient<AddOrderContactsMessage, OrderContactsAdded> addContactsClient)
        {
            _queryService = queryService;
            _createOrderClient = createOrderClient;
            _addContactsClient = addContactsClient;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(await GetCheckoutViewModel(cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> IndexPost(CancellationToken cancellationToken)
        {
            try
            {
                var orderItems = await GetOrderItems(cancellationToken);

                var orderId = Guid.NewGuid();

                await _createOrderClient.Request(
                        new CreateOrderMessage(
                                orderId, 
                                orderItems),
                        cancellationToken)
                    .ConfigureAwait(false);

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
            return View(new AddOrderContactModel{ OrderId = orderId });
        }
        
        [HttpPost]
        public async Task<IActionResult> Contacts(AddOrderContactModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                await _addContactsClient
                    .Request(
                        new AddOrderContactsMessage(
                            model.OrderId,
                            model.Name,
                            model.Email,
                            model.Phone),
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            return View(model);
        }

        private async Task<OrderItem[]> GetOrderItems(CancellationToken cancellationToken)
        {
            var cartItems = await _queryService
                .QueryAsync(
                    new GetCartItems(HttpContext.Session.Id), 
                    cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<OrderItem>>(cartItems).ToArray();
        }

        private async Task<CheckoutViewModel> GetCheckoutViewModel(CancellationToken cancellationToken)
        {
            var cartItems = await _queryService
                .QueryAsync(
                    new GetCartItems(HttpContext.Session.Id),
                    cancellationToken)
                .ConfigureAwait(false);

            var model = new CheckoutViewModel
            {
                CartItems = _mapper.Map<IEnumerable<CartItemModel>>(cartItems)
            };
            return model;
        }
    }
}