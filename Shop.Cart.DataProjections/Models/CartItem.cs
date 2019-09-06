using System;

namespace Shop.Cart.DataProjections.Models
{
    public class CartItem
    {
        public Guid CartItemId { get; set; }

        public string SessionId { get; set; }

        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Count { get; set; }
    }
}