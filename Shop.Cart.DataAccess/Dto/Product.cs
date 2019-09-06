using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shop.Cart.DataAccess.Dto
{
    [Table("Shop_CartProduct")]
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}