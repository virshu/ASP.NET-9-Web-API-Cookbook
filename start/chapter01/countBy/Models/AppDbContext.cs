using Microsoft.EntityFrameworkCore;
namespace CountBy.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
