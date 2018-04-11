using System;

namespace Shop.Domain.Commands.Cart
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