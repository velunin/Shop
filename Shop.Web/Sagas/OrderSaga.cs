using System;
using Automatonymous;
using Marten.Schema;
using MassTransit.Saga;

namespace Shop.Web.Sagas
{
    public class OrderSaga : SagaStateMachineInstance
    {
        public OrderSaga(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        protected OrderSaga()
        {
        }

        [Identity]
        public Guid CorrelationId { get; set; }

        public string CurrenState { get; set; }
    }
}