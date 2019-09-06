using System.Collections.Generic;
using MassInstance.Cqrs.Queries;
using Shop.Catalog.DataProjections.Models;

namespace Shop.Catalog.DataProjections.Queries
{
    public class GetAllProducts : IQuery<IEnumerable<Product>>
    {
    }
}