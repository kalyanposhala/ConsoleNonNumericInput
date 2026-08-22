using KMSS.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KMSS.Api.Infrastructure.Data;

public class KmssDbContext : DbContext
{
    public KmssDbContext(DbContextOptions<KmssDbContext> options) : base(options)
    {
    }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Contribution> Contributions => Set<Contribution>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanRepayment> LoanRepayments => Set<LoanRepayment>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        const int moneyPrecision = 12;
        const int moneyScale = 2;

        modelBuilder.Entity<Member>(e =>
        {
            e.HasIndex(m => m.KhataNo).IsUnique();
            e.Property(m => m.FullName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.HasOne(u => u.Member)
                .WithMany()
                .HasForeignKey(u => u.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Contribution>(e =>
        {
            e.Property(c => c.AmountDue).HasPrecision(moneyPrecision, moneyScale);
            e.Property(c => c.AmountPaid).HasPrecision(moneyPrecision, moneyScale);
            e.HasIndex(c => new { c.MemberId, c.PeriodYear, c.PeriodMonth }).IsUnique();
            e.HasOne(c => c.Member)
                .WithMany(m => m.Contributions)
                .HasForeignKey(c => c.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Loan>(e =>
        {
            e.Property(l => l.PrincipalDisbursed).HasPrecision(moneyPrecision, moneyScale);
            e.Property(l => l.OutstandingPrincipal).HasPrecision(moneyPrecision, moneyScale);
            e.Property(l => l.InterestRatePercent).HasPrecision(5, 2);
            e.HasOne(l => l.Member)
                .WithMany(m => m.Loans)
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.GuarantorMember)
                .WithMany()
                .HasForeignKey(l => l.GuarantorMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoanRepayment>(e =>
        {
            e.Property(r => r.PrincipalPaid).HasPrecision(moneyPrecision, moneyScale);
            e.Property(r => r.InterestPaid).HasPrecision(moneyPrecision, moneyScale);
            e.Property(r => r.OutstandingPrincipalAfter).HasPrecision(moneyPrecision, moneyScale);
            e.HasOne(r => r.Loan)
                .WithMany(l => l.Repayments)
                .HasForeignKey(r => r.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(t => t.Amount).HasPrecision(moneyPrecision, moneyScale);
            e.HasIndex(t => new { t.PeriodYear, t.PeriodMonth, t.Type });
            e.HasOne(t => t.Member)
                .WithMany()
                .HasForeignKey(t => t.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Loan)
                .WithMany()
                .HasForeignKey(t => t.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
