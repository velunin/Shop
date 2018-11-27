using System;

namespace Shop.Web.Models
{
    public class PaymentModel
    {
        public Guid OrderId { get; set; }

        public string Message { get; set; }
    }
}