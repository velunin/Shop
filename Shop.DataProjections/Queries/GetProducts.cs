using System.Collections.Generic;
using MassInstance.Cqrs.Queries;
using Shop.DataProjections.Models;

namespace Shop.DataProjections.Queries
{
    public class GetProducts : IQuery<IEnumerable<Product>>
    {
    }
}