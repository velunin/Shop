using System;
using Shop.Domain.Commands;

namespace Shop.Cart.Domain.Commands
{
    public class DeleteProductFromCart : ICorrelatedCommand
    {
        public DeleteProductFromCart(
            Guid correlationId,
            Guid productId,
            string sessionId)
        {
            CorrelationId = correlationId;
            ProductId = productId;
            SessionId = sessionId;
        }

        public Guid CorrelationId { get; }

        public Guid ProductId { get; }

        public string SessionId { get; }
    }
}