using System.Collections.Generic;
using Shop.Cqrs.Queries;
using Shop.DataProjections.Models;

namespace Shop.DataProjections.Queries
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