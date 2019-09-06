using System;
using MassInstance.Cqrs.Commands;
using Shop.Domain.Commands;
using Shop.Order.Domain.Commands.Results;

namespace Shop.Order.Domain.Commands
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