using System;

namespace Shop.Cart.ServiceModels
{
    public class UpdateCartItemModel
    {
        public string SessionId { get; set; }

        public Guid ProductId { get; set; }

        public int Count { get; set; }
    }
}