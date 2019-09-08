using System;

namespace Shop.Cart.ServiceModels
{
    public class DeleteCartItemModel
    {
        public string SessionId { get; set; }

        public Guid ProductId { get; set; }
    }
}