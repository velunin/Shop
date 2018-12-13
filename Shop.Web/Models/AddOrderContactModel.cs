using System;
using System.ComponentModel.DataAnnotations;

namespace Shop.Web.Models
{
    public class AddOrderContactModel
    {
        public Guid OrderId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }
    }
}