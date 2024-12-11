using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entity;

namespace BasketService.Infrastructure;

public class OrderDatebaseContext : DbContext
{
    public OrderDatebaseContext(DbContextOptions<OrderDatebaseContext> options) : base(options) { }
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

}