﻿using System;
using MassInstance.Cqrs.Events;

namespace Shop.Order.Domain.Events
{
    public class OrderContactsAdded: IEvent
    {
        public OrderContactsAdded(
            Guid orderId, 
            string name, 
            string email, 
            string phone)
        {
            OrderId = orderId;
            CorrelationId = orderId;
            Name = name;
            Email = email;
            Phone = phone;
        }

        public Guid CorrelationId { get; set; } 

        public Guid OrderId { get; }

        public string Name { get; }

        public string Email { get; }

        public string Phone { get; }
    }
}