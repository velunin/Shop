using System;
using Shop.DataProjections.Models;

namespace Shop.DataProjections.SagasContexts
{
    public class OrderSagaContext
    {
        public Guid OrderId { get; set; }

        public OrderStatus CurrentOrderStatus { get; set; }
    }
}