using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LaboratoryEntityConfiguration : IEntityTypeConfiguration<Laboratory>
    {
        public void Configure(EntityTypeBuilder<Laboratory> builder)
        {
            // Table Mapping
            builder.ToTable("Laboratories");

            // Properties
            builder.Property(l => l.Name).IsRequired().HasMaxLength(200);
            builder.Property(l => l.Description).IsRequired(false).HasMaxLength(1000);
            builder.Property(l => l.WhatsAppNumber).IsRequired(false).HasMaxLength(20);
            builder.Property(l => l.Website).IsRequired(false).HasMaxLength(500);
            builder.Property(l => l.LaboratoryStatus).IsRequired().HasConversion<int>().HasDefaultValue(Status.Active);
            builder.Property(l => l.OffersHomeSampleCollection).IsRequired().HasDefaultValue(false);
            builder.Property(l => l.HomeSampleCollectionFee).IsRequired(false).HasPrecision(10, 2);
            builder.Property(l => l.VerificationStatus).IsRequired().HasConversion<int>().HasDefaultValue(VerificationStatus.Unverified);
            builder.Property(l => l.VerifiedAt).IsRequired(false);
            builder.Property(l => l.VerifierId).IsRequired(false);
            builder.Property(l => l.AddressId).IsRequired(false);

            // Ignore NotMapped Properties
            builder.Ignore(l => l.AverageRating);
            builder.Ignore(l => l.TotalReviewsCount);

            // Verifier Relationship (Many-to-One, Optional)
            builder.HasOne(l => l.Verifier)
                .WithMany(v => v.VerifiedLabors)
                .HasForeignKey(l => l.VerifierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Address Relationship (One-to-One, Optional)
            builder.HasOne(l => l.Address)
                .WithOne()
                .HasForeignKey<Laboratory>(l => l.AddressId)
                .OnDelete(DeleteBehavior.SetNull);

            // Verification Documents Relationship (One-to-Many)
            builder.HasMany(l => l.VerificationDocuments)
                .WithOne(ld => ld.Laboratory)
                .HasForeignKey(ld => ld.LaboratoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Working Hours Relationship (One-to-Many)
            builder.HasMany(l => l.WorkingHours)
                .WithOne(wh => wh.Laboratory)
                .HasForeignKey(wh => wh.LaboratoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lab Services Relationship (One-to-Many)
            builder.HasMany(l => l.LabServices)
                .WithOne(ls => ls.Laboratory)
                .HasForeignKey(ls => ls.LaboratoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lab Orders Relationship (One-to-Many)
            builder.HasMany(l => l.LabOrders)
                .WithOne(lo => lo.Laboratory)
                .HasForeignKey(lo => lo.LaboratoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Laboratory Reviews Relationship (One-to-Many)
            builder.HasMany(l => l.LaboratoryReviews)
                .WithOne(lr => lr.Laboratory)
                .HasForeignKey(lr => lr.LaboratoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(l => l.Name)
                .HasDatabaseName("IX_Laboratory_Name");

            builder.HasIndex(l => l.VerificationStatus)
                .HasDatabaseName("IX_Laboratory_VerificationStatus");

            builder.HasIndex(l => l.AddressId)
                .HasDatabaseName("IX_Laboratory_AddressId");

            builder.HasIndex(l => new { l.LaboratoryStatus, l.OffersHomeSampleCollection })
                .HasDatabaseName("IX_Laboratory_Status_HomeCollection");

            // Check Constraint
            builder.HasCheckConstraint("CK_Laboratory_HomeSampleFee", "[HomeSampleCollectionFee] IS NULL OR [HomeSampleCollectionFee] >= 0");
        }
	}
}
