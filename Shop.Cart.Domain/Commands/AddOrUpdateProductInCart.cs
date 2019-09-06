﻿using System;
using MassInstance.Cqrs.Commands;

namespace Shop.Cart.Domain.Commands
{
    public class AddOrUpdateProductInCart : ICommand
    {
        public AddOrUpdateProductInCart(
            Guid correlationId, 
            Guid productId, 
            string sessionId,
            int count)
        {
            if (count < 1)
            {
                throw new ArgumentException("Value must greater than 0", nameof(count));
            }

            CorrelationId = correlationId;
            ProductId = productId;
            SessionId = sessionId;
            Count = count;
        }

        public Guid CorrelationId { get; }

        public Guid ProductId { get; }

        public string SessionId { get; }

        public int Count { get; }
    }
}