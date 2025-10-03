using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LaboratoryEntityConfiguration : IEntityTypeConfiguration<Laboratory>
    {
        public void Configure(EntityTypeBuilder<Laboratory> builder)
        {
            builder.Property(l => l.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(l => l.Description)
                   .HasMaxLength(1000);

            builder.Property(l => l.WhatsAppNumber)
                   .HasMaxLength(20);

            builder.Property(l => l.Website)
                   .HasMaxLength(200);

            builder.Property(l => l.LaboratoryStatus)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(LaboratoryStatus.Active);

			builder.Property(l => l.OffersHomeSampleCollection)
				   .IsRequired()
				   .HasDefaultValue(false);

			builder.Property(l => l.HomeSampleCollectionFee)
                   .HasPrecision(10, 2);


            builder.Property(l => l.VerificationStatus)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(LaboratoryVerificationStatus.Unverified);

            // Relationships
            builder.HasOne(l => l.Verifier)
                   .WithMany()
                   .HasForeignKey(l => l.VerifierId)
                   .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(l => l.Address)
	               .WithOne()
	               .HasForeignKey<Laboratory>(l => l.AddressId)
	               .OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(l => l.VerificationDocuments)
                   .WithOne(ld => ld.Laboratory)
                   .HasForeignKey(ld => ld.LaboratoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.WorkingHours)
                   .WithOne(wh => wh.Laboratory)
                   .HasForeignKey(wh => wh.LaboratoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.LabServices)
                   .WithOne(ls => ls.Laboratory)
                   .HasForeignKey(ls => ls.LaboratoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.LabOrders)
                   .WithOne(lo => lo.Laboratory)
                   .HasForeignKey(lo => lo.LaboratoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(l => l.Name).HasDatabaseName("IX_Laboratory_Name");

            // Constraints
			builder.HasCheckConstraint("CK_Laboratory_HomeSampleFee", "[HomeSampleCollectionFee] IS NULL OR [HomeSampleCollectionFee] >= 0");

		}
	}
}
