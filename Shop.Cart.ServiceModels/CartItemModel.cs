﻿using System;

namespace Shop.Cart.ServiceModels
{
    public class CartItemModel
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Count { get; set; }
    }
}