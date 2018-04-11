using System;
using Shop.Domain.Sagas;

namespace Shop.Domain.Commands.Order
{
    public class AddOrderContactsMessage : CorrelationCommandMessage<AddOrderContactsCommand>
    {
        public AddOrderContactsMessage(Guid orderId, string name, string email, string phone)
            : base(
                new AddOrderContactsCommand(
                    orderId,
                    name,
                    email,
                    phone))
        {
        }
    }
}