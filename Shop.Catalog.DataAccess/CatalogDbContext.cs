using Microsoft.EntityFrameworkCore;
using Shop.Catalog.DataAccess.Dto;

namespace Shop.Catalog.DataAccess
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}