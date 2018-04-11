using System;

namespace Shop.Web.Models
{
    public class AddToCartModel
    {
        public Guid ProductId { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class DeleteFromCartModel
    {
        public Guid ProductId { get; set; }

        public string ReturnUrl { get; set; }
    }
}