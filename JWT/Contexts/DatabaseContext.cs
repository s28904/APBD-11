using JWT.Models;
using Microsoft.EntityFrameworkCore;

namespace JWT.Contexts;

public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    protected DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }
}