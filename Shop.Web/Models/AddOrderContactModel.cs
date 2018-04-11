using System;

namespace Shop.Web.Models
{
    public class AddOrderContactModel
    {
        public Guid OrderId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}