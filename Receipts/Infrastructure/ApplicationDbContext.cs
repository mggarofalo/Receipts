using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	public DbSet<AccountEntity> Accounts { get; set; } = null!;
	public DbSet<ReceiptEntity> Receipts { get; set; } = null!;
	public DbSet<TransactionEntity> Transactions { get; set; } = null!;
	public DbSet<TransactionItemEntity> TransactionItems { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<AccountEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.AccountCode).IsRequired();
			entity.Property(e => e.Name).IsRequired();
		});

		modelBuilder.Entity<ReceiptEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Location).IsRequired();
			entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
			entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
		});

		modelBuilder.Entity<TransactionEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
			entity.HasOne(e => e.Receipt)
				.WithMany()
				.HasForeignKey(e => e.ReceiptId)
				.OnDelete(DeleteBehavior.Cascade);
			entity.HasOne(e => e.Account)
				.WithMany()
				.HasForeignKey(e => e.AccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<TransactionItemEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
			entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
			entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
			entity.HasOne(e => e.Transaction)
				.WithMany()
				.HasForeignKey(e => e.TransactionId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}
