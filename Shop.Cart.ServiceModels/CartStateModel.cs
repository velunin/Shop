using System.Collections.Generic;

namespace Shop.Cart.ServiceModels
{
    public class CartStateModel
    {
        public IEnumerable<CartItemModel> Items { get; set; }

        public decimal Sum { get; set; }
    }
}