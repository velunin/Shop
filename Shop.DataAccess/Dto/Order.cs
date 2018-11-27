using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shop.DataProjections.Models;

namespace Shop.DataAccess.Dto
{
    [Table("Shop_Order")]
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreateDate { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}