﻿using System;

namespace Shop.Domain.Commands.Order
{
    public class AddOrderContactsCommand : ICorrelatedCommand
    {
        public AddOrderContactsCommand(Guid orderId, string name, string email, string phone)
        {
            OrderId = orderId;
            Name = name;
            Email = email;
            Phone = phone;
        }

        public Guid CorrelationId => OrderId;

        public Guid OrderId { get; }

        public string Name { get; }

        public string Email { get; }

        public string Phone { get; }
    }
}