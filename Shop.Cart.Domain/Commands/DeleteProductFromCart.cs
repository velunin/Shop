using System;
using MassInstance.Cqrs.Commands;

namespace Shop.Cart.Domain.Commands
{
    public class DeleteProductFromCart : ICommand
    {
        public DeleteProductFromCart(
            Guid productId,
            string sessionId)
        {
            ProductId = productId;
            SessionId = sessionId;
        }

        public Guid ProductId { get; }

        public string SessionId { get; }
    }
}