using System;

namespace Shop.Catalog.ServiceModels
{
    public class ProductModel
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}