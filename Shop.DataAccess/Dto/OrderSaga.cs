using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Automatonymous;

namespace Shop.DataAccess.Dto
{
    [Table("Shop_OrderSaga")]
    public class OrderSaga : SagaStateMachineInstance
    {
        public OrderSaga(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        protected OrderSaga()
        {
        }

        [Key]
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}