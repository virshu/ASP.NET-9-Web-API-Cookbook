using Microsoft.EntityFrameworkCore;
using ProblemDetailsDemo.Models;

namespace ProblemDetailsDemo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
