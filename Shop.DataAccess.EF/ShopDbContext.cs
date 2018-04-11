using Microsoft.EntityFrameworkCore;
using Shop.DataAccess.Dto;
using Shop.DataAccess.EF.NSaga;

namespace Shop.DataAccess.EF
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }

        public DbSet<SagaData> Sagas { get; set; }

        public DbSet<SagaHeaders> SagaHeaders { get; set; }

        public DbSet<CartItem> CartItem { get; set; }
    }
}