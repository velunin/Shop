using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassInstance.Bus;
using MassInstance.Cqrs.Commands;
using MassInstance.Cqrs.Queries;
using Microsoft.AspNetCore.Mvc;
using Shop.Cart.DataProjections.Queries;
using Shop.Cart.Domain.Commands;
using Shop.Cart.ServiceModels;
using Shop.Order.Domain.Commands;
using Shop.Order.Domain.Commands.Dto;

namespace Shop.Cart.ServiceEndpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IQueryService _queryService;
        private readonly ICommandProcessor _commandProcessor;
        private readonly IServiceBus _bus;
        private readonly IMapper _mapper;

        public CartController(
            IQueryService queryService, 
            ICommandProcessor commandProcessor,
            IServiceBus bus,
            IMapper mapper)
        {
            _queryService = queryService;
            _mapper = mapper;
            _bus = bus;
            _commandProcessor = commandProcessor;
        }

        [Route("{sessionId}")]
        public async Task<CartStateModel> Get(string sessionId, CancellationToken cancellationToken)
        {
            return await ConstructCartModel(sessionId, cancellationToken);
        }

        [HttpPost]
        public async Task<CartStateModel> Update(
            UpdateCartItemModel updateCartItemModel,
            CancellationToken cancellationToken)
        {
            await _commandProcessor.ProcessAsync(
                new AddOrUpdateProductInCart(
                    updateCartItemModel.ProductId,
                    updateCartItemModel.SessionId,
                    updateCartItemModel.Count),
                cancellationToken);

            return await ConstructCartModel(updateCartItemModel.SessionId, cancellationToken);
        }

        public async Task<CartStateModel> Delete(DeleteCartItemModel deleteCartItemModel, CancellationToken cancellationToken)
        {
            await _commandProcessor.ProcessAsync(
                new DeleteProductFromCart(
                    deleteCartItemModel.ProductId,
                    deleteCartItemModel.SessionId), 
                cancellationToken);

            return await ConstructCartModel(deleteCartItemModel.SessionId, cancellationToken);
        }

        [HttpPost]
        public async Task<Guid> Checkout(string sessionId, CancellationToken cancellationToken)
        {
            var orderId = Guid.NewGuid();
            
            await _bus.ProcessAsync(
                new CreateOrderCommand(
                    orderId,
                    await GetOrderItems(sessionId, cancellationToken)),
                TimeSpan.FromSeconds(10),
                cancellationToken);

            return orderId;
        }

        private async Task<CartStateModel> ConstructCartModel(string sessionId, CancellationToken cancellationToken)
        {
            var cartItems = await _queryService.QueryAsync(new GetCartItems(sessionId), cancellationToken);

            decimal sum = 0;

            var cartItemModels = cartItems.Select(ci =>
            {
                sum += ci.Price * ci.Count;

                return _mapper.Map<CartItemModel>(ci);
            });

            return new CartStateModel
            {
                Items = cartItemModels,
                Sum = sum
            };
        }

        private async Task<OrderItem[]> GetOrderItems(string sessionId, CancellationToken cancellationToken)
        {
            var cartItems = await _queryService
                .QueryAsync(
                    new GetCartItems(sessionId),
                    cancellationToken);

            return _mapper.Map<IEnumerable<OrderItem>>(cartItems).ToArray();
        }
    }
}