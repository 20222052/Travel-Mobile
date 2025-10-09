using Microsoft.EntityFrameworkCore;
using project.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> User { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<Contact> Contact { get; set; }
    public DbSet<Tour> Tour { get; set; }
    public DbSet<Blog> Blog { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<History> History { get; set; }
    public DbSet<Cart> Cart { get; set; }
    public DbSet<CartItem> CartItem { get; set; }
    public DbSet<OrderDetail> OrderDetail { get; set; }
}

