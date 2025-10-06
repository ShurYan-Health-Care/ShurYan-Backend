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
			builder.ToTable("Pharmacies");

			builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(250);

            builder.HasOne(p => p.Address)
                .WithOne()
                .HasForeignKey<Pharmacy>(p => p.AddressId)
                .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(p => p.Verifier)
                .WithMany(v => v.VerifiedPharmacies)
                .HasForeignKey(p => p.VerifierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.VerificationDocuments)
                .WithOne(d => d.Pharmacy)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.WorkingHours)
                .WithOne(wh => wh.Pharmacy)
                .HasForeignKey(wh => wh.PharmacyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Orders)
                .WithOne(o => o.Pharmacy)
                .HasForeignKey(o => o.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

			builder.Property(p => p.PharmacyStatus)
				   .HasConversion<int>()
				   .IsRequired()
				   .HasDefaultValue(Status.Active);

			builder.HasMany(p => p.PharmacyReviews)
	                .WithOne(pr => pr.Pharmacy)
	                .HasForeignKey(pr => pr.PharmacyId)
	                .OnDelete(DeleteBehavior.Restrict);
		}
    }
}
