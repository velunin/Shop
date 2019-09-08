using Microsoft.EntityFrameworkCore;
using Shop.Order.DataAccess.Dto;

namespace Shop.Order.DataAccess
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OrderSagaContext> OrderSagaContexts { get; set; }

        public DbSet<Dto.Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItem { get; set; }
    }
}