using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Payments;

namespace Shuryan.Infrastructure.Data.Configurations.PaymentConfigurations
{
    public class PaymentTransactionEntityConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TransactionType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.ProviderTransactionId)
                .HasMaxLength(200);

            builder.Property(t => t.ProviderResponse)
                .HasMaxLength(1000);

            builder.Property(t => t.ErrorMessage)
                .HasMaxLength(500);

            builder.Property(t => t.ErrorCode)
                .HasMaxLength(50);

            builder.Property(t => t.Metadata)
                .HasMaxLength(1000);

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(t => t.Payment)
                .WithMany(p => p.Transactions)
                .HasForeignKey(t => t.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.PaymentId);
            builder.HasIndex(t => t.ProviderTransactionId);
            builder.HasIndex(t => t.CreatedAt);
        }
    }
}
