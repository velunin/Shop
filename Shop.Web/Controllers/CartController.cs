using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using MassInstance.Client;
using MassInstance.Cqrs.Queries;
using Microsoft.AspNetCore.Mvc;
using Shop.DataProjections.Queries;
using Shop.Domain.Commands.Cart;
using Shop.Services.Common;
using Shop.Web.Models;

namespace Shop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IQueryService _queryService;
        private readonly IMapper _mapper;
        private readonly IServiceClient _serviceClient;

        public CartController(
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
            var cartItems = await _queryService.QueryAsync(
                    new GetCartItems(HttpContext.Session.Id),
                    cancellationToken);

            var model = _mapper.Map<IEnumerable<CartItemModel>>(cartItems);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddToCartModel addToCart, CancellationToken cancellationToken)
        {
            await _serviceClient.ProcessAsync(
                new AddOrUpdateProductInCart(
                    Guid.NewGuid(), 
                    addToCart.ProductId,
                    HttpContext.Session.Id, 1), 
                CancellationToken.None);

            return Redirect(addToCart.ReturnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeleteFromCartModel deleteFromCart, CancellationToken cancellationToken)
        {
            await _serviceClient.ProcessAsync(
                new DeleteProductFromCart(
                    Guid.NewGuid(),
                    deleteFromCart.ProductId,
                    HttpContext.Session.Id),
                CancellationToken.None);

            return Redirect(deleteFromCart.ReturnUrl);
        }
    }
}