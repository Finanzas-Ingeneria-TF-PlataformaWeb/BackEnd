using Microsoft.EntityFrameworkCore;
using MiVivienda.Api.Models;

namespace MiVivienda.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Simulation> Simulations => Set<Simulation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.DocumentNumber)
            .IsUnique();

        modelBuilder.Entity<Property>()
            .HasIndex(p => p.Code)
            .IsUnique();
    }
}