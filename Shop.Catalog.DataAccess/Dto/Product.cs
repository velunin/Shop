using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shop.Catalog.DataAccess.Dto
{
    [Table("Shop_Product")]
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}