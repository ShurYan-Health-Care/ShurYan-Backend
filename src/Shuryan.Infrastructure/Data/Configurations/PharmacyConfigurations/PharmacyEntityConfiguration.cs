using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PharmacyConfigurations
{
    public class PharmacyEntityConfiguration : IEntityTypeConfiguration<Pharmacy>
    {
        public void Configure(EntityTypeBuilder<Pharmacy> builder)
        {
            // Table Mapping
            builder.ToTable("Pharmacies");

            // Properties
            builder.Property(p => p.Name).IsRequired().HasMaxLength(250);
            builder.Property(p => p.Description).IsRequired(false).HasMaxLength(1000);
            builder.Property(p => p.WhatsAppNumber).IsRequired(false).HasMaxLength(20);
            builder.Property(p => p.Website).IsRequired(false).HasMaxLength(500);
            builder.Property(p => p.PharmacyStatus).IsRequired().HasConversion<int>().HasDefaultValue(Status.Active);
            builder.Property(p => p.OffersDelivery).IsRequired().HasDefaultValue(true);
            builder.Property(p => p.VerificationStatus).IsRequired().HasConversion<int>().HasDefaultValue(VerificationStatus.Unverified);
            builder.Property(p => p.VerifiedAt).IsRequired(false);
            builder.Property(p => p.VerifierId).IsRequired(false);
            builder.Property(p => p.AddressId).IsRequired(false);

            // Ignore NotMapped Properties
            builder.Ignore(p => p.AverageRating);
            builder.Ignore(p => p.TotalReviewsCount);

            // Address Relationship (One-to-One, Optional)
            builder.HasOne(p => p.Address)
                .WithOne()
                .HasForeignKey<Pharmacy>(p => p.AddressId)
                .OnDelete(DeleteBehavior.SetNull);

            // Verifier Relationship (Many-to-One, Optional)
            builder.HasOne(p => p.Verifier)
                .WithMany(v => v.VerifiedPharmacies)
                .HasForeignKey(p => p.VerifierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Verification Documents Relationship (One-to-Many)
            builder.HasMany(p => p.VerificationDocuments)
                .WithOne(d => d.Pharmacy)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Working Hours Relationship (One-to-Many)
            builder.HasMany(p => p.WorkingHours)
                .WithOne(wh => wh.Pharmacy)
                .HasForeignKey(wh => wh.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Orders Relationship (One-to-Many)
            builder.HasMany(p => p.Orders)
                .WithOne(o => o.Pharmacy)
                .HasForeignKey(o => o.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pharmacy Reviews Relationship (One-to-Many)
            builder.HasMany(p => p.PharmacyReviews)
                .WithOne(pr => pr.Pharmacy)
                .HasForeignKey(pr => pr.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Pharmacy_Name");

            builder.HasIndex(p => p.VerificationStatus)
                .HasDatabaseName("IX_Pharmacy_VerificationStatus");

            builder.HasIndex(p => p.AddressId)
                .HasDatabaseName("IX_Pharmacy_AddressId");

            builder.HasIndex(p => new { p.PharmacyStatus, p.OffersDelivery })
                .HasDatabaseName("IX_Pharmacy_Status_Delivery");
        }
    }
}
