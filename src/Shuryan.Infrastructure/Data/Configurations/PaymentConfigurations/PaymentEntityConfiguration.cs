using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Payments;

namespace Shuryan.Infrastructure.Data.Configurations.PaymentConfigurations
{
    public class PaymentEntityConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.OrderType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("EGP");

            builder.Property(p => p.PaymentMethod)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.Provider)
                .HasConversion<int>();

            builder.Property(p => p.ProviderTransactionId)
                .HasMaxLength(200);

            builder.Property(p => p.ProviderResponse)
                .HasMaxLength(500);

            builder.Property(p => p.RefundedAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.RefundReason)
                .HasMaxLength(500);

            builder.Property(p => p.Notes)
                .HasMaxLength(1000);

            builder.Property(p => p.IpAddress)
                .HasMaxLength(100);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Transactions)
                .WithOne(t => t.Payment)
                .HasForeignKey(t => t.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => new { p.OrderType, p.OrderId });
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.ProviderTransactionId);
            builder.HasIndex(p => p.CreatedAt);
        }
    }
}
