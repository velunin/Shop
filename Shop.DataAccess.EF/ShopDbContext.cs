using Microsoft.EntityFrameworkCore;
using Shop.DataAccess.Dto;

namespace Shop.DataAccess.EF
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }

        public DbSet<CartItem> CartItem { get; set; }

        public DbSet<OrderSagaContext> OrderSaga { get; set; }

        public DbSet<Order> Order { get; set; }

        public DbSet<OrderItem> OrderItem { get; set; }
    }
}