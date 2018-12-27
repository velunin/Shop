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

        public string CreateOrderRequestIdentity { get; set; }

        public void SetCreateOrderRequestIdentity(Guid? requestId, string responseAddress)
        {
            if (requestId.HasValue)
            {
                CreateOrderRequestIdentity = $"{responseAddress}|{requestId}";
            }
        }

        public (Guid?,string) GetCreateOrderRequestIdentity()
        {
            if (!string.IsNullOrEmpty(CreateOrderRequestIdentity))
            {
                var requestIdentityPair = CreateOrderRequestIdentity.Split('|');

                if (requestIdentityPair != null && requestIdentityPair.Length > 0)
                {
                    var responseAddress = requestIdentityPair[0];
                    var requestIdStr = requestIdentityPair[1];

                    if (Guid.TryParse(requestIdStr, out var requestId))
                    {
                        return (requestId, responseAddress);
                    }
                } 
            }

            return (null, null);
        }
    }
}