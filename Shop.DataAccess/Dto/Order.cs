using System;
using System.ComponentModel.DataAnnotations;
using Shop.DataProjections.Models;

namespace Shop.DataAccess.Dto
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreateDate { get; set; }
    }
}