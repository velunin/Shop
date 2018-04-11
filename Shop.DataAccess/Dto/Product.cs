using System;
using System.ComponentModel.DataAnnotations;

namespace Shop.DataAccess.Dto
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}