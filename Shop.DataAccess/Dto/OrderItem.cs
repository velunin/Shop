using System;
using System.ComponentModel.DataAnnotations;

namespace Shop.DataAccess.Dto
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Count { get; set; }
    }
}