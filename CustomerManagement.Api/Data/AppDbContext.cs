using CustomerManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var customer = modelBuilder.Entity<Customer>();

        customer.ToTable("Customers");

        customer.HasKey(x => x.Id);

        customer.Property(x => x.CustomerCode)
            .HasMaxLength(20)
            .IsRequired();

        customer.HasIndex(x => x.CustomerCode)
            .IsUnique();

        customer.Property(x => x.FullName)
            .HasMaxLength(150)
            .IsRequired();

        customer.Property(x => x.Email)
            .HasMaxLength(254);

        customer.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        customer.Property(x => x.DateOfBirth)
            .HasColumnType("date");

        customer.Property(x => x.IsActive)
            .IsRequired();
    }
}