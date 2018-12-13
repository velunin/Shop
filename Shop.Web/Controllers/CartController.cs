using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using Rds.Cqrs.Commands;
using Rds.Cqrs.Queries;

using Shop.DataProjections.Queries;
using Shop.Domain.Commands.Cart;
using Shop.Web.Models;

namespace Shop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICommandProcessor _commandProcessor;
        private readonly IQueryService _queryService;
        private readonly IMapper _mapper;

        public CartController(
            ICommandProcessor commandProcessor,
            IQueryService queryService, 
            IMapper mapper)
        {
            _commandProcessor = commandProcessor;
            _queryService = queryService;
            _mapper = mapper;
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
            await _commandProcessor.ProcessAsync(
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
            await _commandProcessor.ProcessAsync(
                new DeleteProductFromCart(
                    Guid.NewGuid(),
                    deleteFromCart.ProductId,
                    HttpContext.Session.Id),
                CancellationToken.None);

            return Redirect(deleteFromCart.ReturnUrl);
        }
    }
}