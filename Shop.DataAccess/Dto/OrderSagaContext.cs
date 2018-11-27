using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Automatonymous;

namespace Shop.DataAccess.Dto
{
    [Table("Shop_OrderSaga")]
    public class OrderSagaContext : SagaStateMachineInstance
    {
        public OrderSagaContext(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        protected OrderSagaContext()
        {
        }

        [Key]
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}