using Microsoft.EntityFrameworkCore;
using Shop.Cart.DataAccess.Dto;

namespace Shop.Cart.DataAccess
{
    public class CartDbContext : DbContext
    {
        public CartDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Product> Products { get; set; }
    }
}