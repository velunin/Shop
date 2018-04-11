using System;

namespace Shop.Web.Models
{
    public class ProductViewModel
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int CountInCart { get; set; }
    }
}