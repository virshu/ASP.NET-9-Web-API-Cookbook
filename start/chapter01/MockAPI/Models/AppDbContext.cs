using Microsoft.EntityFrameworkCore;

namespace mockAPI.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
