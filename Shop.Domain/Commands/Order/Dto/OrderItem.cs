using System;

namespace Shop.Domain.Commands.Order
{
    public class OrderItem
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Count { get; set; }
    }
}