using System;
using MassInstance.Cqrs.Commands;
using Shop.Domain.Commands.Order.Results;

namespace Shop.Domain.Commands.Order
{
    public class PayOrderCommand : IResultingCommand<PayOrderResult>, ICorrelatedCommand
    {
        public PayOrderCommand(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; }

        public Guid CorrelationId => OrderId;
    }
}