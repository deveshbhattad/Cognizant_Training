// Data/TreasuryManagementSystemContext.cs
using Microsoft.EntityFrameworkCore;
using TMS_MAIN.Models;
using System;

namespace TMS_MAIN.Data
{
    public class TreasuryManagementSystemContext : DbContext
    {
        public TreasuryManagementSystemContext(DbContextOptions<TreasuryManagementSystemContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CashFlow> CashFlows { get; set; }
        public DbSet<Risk> RiskAssessments { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Investment> Investments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Compliance> Compliances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=LTIN522006\\SQLEXPRESS;Database=TMSMain;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure all decimal properties explicitly
            modelBuilder.Entity<Investment>(entity =>
            {
                entity.Property(i => i.AmountInvested).HasColumnType("decimal(18,2)");
                entity.Property(i => i.CurrentValue).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<BankAccount>(entity =>
            {
                // These will now resolve if the BankAccount model has the 'Balance' property
                entity.Property(b => b.Balance).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<CashFlow>(entity =>
            {
                entity.Property(c => c.Amount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Risk>(entity =>
            {
                entity.Property(r => r.Amount).HasColumnType("decimal(18,2)");
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsAdmin).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(15);
                entity.Property(e => e.Address).HasMaxLength(200);
            });

            // CashFlow relationships
            modelBuilder.Entity<CashFlow>()
                .HasOne(cf => cf.User)
                .WithMany(u => u.CashFlows)
                .HasForeignKey(cf => cf.UserId)
                //.OnDelete(DeleteBehavior.Restrict);
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CashFlow>()
                .HasOne(c => c.BankAccount)
                .WithMany(b => b.CashFlows) // This now refers to BankAccount.CashFlows
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // BankAccount relationships
            modelBuilder.Entity<BankAccount>()
               .HasOne(b => b.User) // This now refers to BankAccount.User
               .WithMany(u => u.BankAccounts)
               .HasForeignKey(b => b.UserId)
               //.OnDelete(DeleteBehavior.Restrict);
               .OnDelete(DeleteBehavior.Cascade);

            // Investment relationship
            modelBuilder.Entity<Investment>()
                .HasOne(i => i.User)
                .WithMany(u => u.Investments)
                .HasForeignKey(i => i.UserId)
                //.OnDelete(DeleteBehavior.Restrict);
                .OnDelete(DeleteBehavior.Cascade);

            // Report configuration
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(r => r.ReportId);
                entity.Property(r => r.ReportName).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Module).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Parameters).IsRequired(false);
                entity.Property(r => r.Status).HasMaxLength(50).HasDefaultValue("Generated");

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reports)
                    .HasForeignKey(r => r.UserId)
                    //.OnDelete(DeleteBehavior.Restrict);
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Compliance configuration
            modelBuilder.Entity<Compliance>(entity =>
            {
                entity.HasKey(c => c.ComplianceId);
                entity.Property(c => c.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Submitted");
                entity.Property(c => c.Notes).HasMaxLength(500);

                entity.HasOne(c => c.Report)
                    .WithMany(r => r.Compliances)
                    .HasForeignKey(c => c.ReportId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Compliances)
                    .HasForeignKey(c => c.UserId)
                    //.OnDelete(DeleteBehavior.Restrict);
                    .OnDelete(DeleteBehavior.Cascade);
            });

// Seed data for Users
modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    Password = "admin",
                    IsAdmin = true,
                    FullName = "System Administrator",
                    Email = "admin@company.com",
                    PhoneNumber = "",
                    Address = ""
                },
                new User
                {
                    UserId = 2,
                    Username = "treasurer",
                    Password = "treasurer",
                    IsAdmin = false,
                    FullName = "Default Treasurer",
                    Email = "treasurer@company.com",
                    PhoneNumber = "123456789",
                    Address = "bnc"
                },
                new User
                {
                    UserId = 3,
                    Username = "treasurer1",
                    Password = "treasurer",
                    IsAdmin = false,
                    FullName = "Default Treasurer",
                    Email = "treasurer1@company.com",
                    PhoneNumber = "583456789",
                    Address = "mysuru"
                },
                new User
                {
                    UserId = 4,
                    Username = "treasurer2",
                    Password = "treasurer",
                    IsAdmin = false,
                    FullName = "Default Treasurer",
                    Email = "treasurer1@company.com",
                    PhoneNumber = "583456789",
                    Address = "Bengaluru"
                }
            );

            // Seed data for BankAccounts
            modelBuilder.Entity<BankAccount>().HasData(
                new BankAccount
                {
                    AccountId = 1,
                    BankName = "Chase Bank",
                    AccountNumber = "1234567890",
                    AccountType = AccountType.Savings, // This will now resolve
                    Balance = 50000.00m,               // This will now resolve
                    UserId = 2                         // This will now resolve
                },
                new BankAccount
                {
                    AccountId = 2,
                    BankName = "Bank of America",
                    AccountNumber = "2345678901",
                    AccountType = AccountType.Checking, // This will now resolve
                    Balance = 15000.00m,                // This will now resolve
                    UserId = 2                          // This will now resolve
                },
                new BankAccount
                {
                    AccountId = 3,
                    BankName = "Wells Fargo",
                    AccountNumber = "3456789012",
                    AccountType = AccountType.Savings, // This will now resolve
                    Balance = 12000.00m,               // This will now resolve
                    UserId = 1                         // This will now resolve
                },
                new BankAccount
                {
                    AccountId = 4,
                    BankName = "Citibank",
                    AccountNumber = "4567890123",
                    AccountType = AccountType.Checking, // This will now resolve
                    Balance = 25000.00m,                // This will now resolve
                    UserId = 1                          // This will now resolve
                }
            );

            // Seed data for CashFlows
            modelBuilder.Entity<CashFlow>().HasData(
                new CashFlow
                {
                    TransactionId = 1,
                    Amount = 50000.00m,
                    TransactionType = TransactionType.Inflow,
                    TransactionDate = new DateTime(2025, 2, 10),
                    Description = "service tax",
                    UserId = 2,
                    AccountId = 1
                },
                new CashFlow
                {
                    TransactionId = 2,
                    Amount = 15000.00m,
                    TransactionType = TransactionType.Outflow,
                    TransactionDate = new DateTime(2025, 2, 15),
                    Description = "Office Supplies",
                    UserId = 2,
                    AccountId = 2
                },
                new CashFlow
                {
                    TransactionId = 3,
                    Amount = 12000.00m,
                    TransactionType = TransactionType.Inflow,
                    TransactionDate = new DateTime(2025, 3, 1),
                    Description = "Project Payment - Alpha",
                    UserId = 1,
                    AccountId = 3
                },
                new CashFlow
                {
                    TransactionId = 4,
                    Amount = 25000.00m,
                    TransactionType = TransactionType.Inflow,
                    TransactionDate = new DateTime(2025, 3, 10),
                    Description = "Consulting Fee",
                    UserId = 1,
                    AccountId = 4
                }
            );

            // Seed data for Investments
            modelBuilder.Entity<Investment>().HasData(
                new Investment
                {
                    InvestmentId = 1,
                    InvestmentType = InvestmentType.Bonds,
                    AmountInvested = 10000,
                    CurrentValue = 10500,
                    PurchaseDate = new DateTime(2024, 1, 15),
                    MaturityDate = new DateTime(2026, 1, 15),
                    UserId = 2
                },
                new Investment
                {
                    InvestmentId = 2,
                    InvestmentType = InvestmentType.MutualFunds,
                    AmountInvested = 20000,
                    CurrentValue = 25000,
                    PurchaseDate = new DateTime(2023, 6, 10),
                    MaturityDate = new DateTime(2025, 6, 10),
                    UserId = 3
                }
            );

            // Seed data for Reports
            modelBuilder.Entity<Report>().HasData(
                new Report
                {
                    ReportId = 1,
                    ReportName = "Quarterly Cash Flow",
                    Module = "CashFlow",
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 3, 31),
                    GeneratedDate = new DateTime(2025, 4, 1),
                    UserId = 2,
                    Parameters = "{\"AccountId\":1}",
                    Status = "Generated"
                },
                new Report
                {
                    ReportId = 2,
                    ReportName = "Investment Portfolio",
                    Module = "Investment",
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 3, 31),
                    GeneratedDate = new DateTime(2025, 4, 1),
                    UserId = 2,
                    Parameters = "{}",
                    Status = "Viewed"
                }
                );
         }
    }
}