using System;
using Shop.Domain.Order.Commands.Results;

namespace Shop.Domain.Order.Commands
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