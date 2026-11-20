using ATM.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Data;

public class ATMDbContext : DbContext
{
  public ATMDbContext(DbContextOptions<ATMDbContext> options)
    : base(options)
  {
  }

  public ATMDbContext()
  {
  }

  public DbSet<User> Users { get; set; }
  public DbSet<Account> Accounts { get; set; }
  public DbSet<Transaction> Transactions { get; set; }
  public DbSet<LoanRequest> LoanRequests { get; set; }
  public DbSet<PasswordRecovery> PasswordRecoveries { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
      optionsBuilder.UseSqlServer(
        @"Server=localhost,1433;
         Database=ATMDb;
         User Id=sa;
         Password=AtmProject2026@;
         TrustServerCertificate=True;"
      );
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<User>(entity =>
    {
      entity.Property(u => u.Username)
        .IsRequired()
        .HasMaxLength(50);

      entity.Property(u => u.LastName)
        .IsRequired()
        .HasMaxLength(50);

      entity.Property(u => u.Username)
        .IsRequired()
        .HasMaxLength(50);

      entity.HasIndex(u => u.Username)
        .IsUnique();

      entity.Property(u => u.PasswordHash)
        .IsRequired();

      entity.Property(u => u.Role)
        .HasConversion<string>();
    });

    modelBuilder.Entity<Account>(entity =>
    {
      entity.Property(a => a.Balance)
        .HasPrecision(18, 2);
    });

    modelBuilder.Entity<Transaction>(entity =>
    {
      entity.Property(t => t.Amount)
        .HasPrecision(18, 2);

      entity.Property(t => t.Type)
        .HasConversion<string>();
    });

    modelBuilder.Entity<LoanRequest>(entity =>
    {
      entity.Property(l => l.Amount)
        .HasPrecision(18, 2);

      entity.Property(l => l.Status)
        .HasConversion<string>();
    });

    modelBuilder.Entity<PasswordRecovery>(entity =>
    {
      entity.Property(r => r.CodeHash)
        .IsRequired();

      entity.HasIndex(r => r.UserId)
        .IsUnique();
    });
  }
}
