using System.Collections.Generic;
using MassInstance.Cqrs.Queries;
using Shop.Cart.DataProjections.Models;

namespace Shop.Cart.DataProjections.Queries
{
    public class GetCartItems : IQuery<IEnumerable<CartItem>>
    {
        public GetCartItems(string sessionId)
        {
            SessionId = sessionId;
        }

        public string SessionId { get; }
    }
}