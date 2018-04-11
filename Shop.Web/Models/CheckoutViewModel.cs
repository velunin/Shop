using System.Collections.Generic;
using System.Linq;

namespace Shop.Web.Models
{
    public class CheckoutViewModel
    {
        public IEnumerable<CartItemModel> CartItems { get; set; }

        public decimal Sum
        {
            get
            {
                if (CartItems != null && CartItems.Any())
                {
                    return CartItems.Sum(x => x.Price * x.Count);
                }

                return 0;
            }
        }
    }
}