using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Data.Configurations.PrescriptionConfigurations
{
    public class MedicationEntityConfiguration : AuditableEntityConfiguration<Medication>
    {
        public override void Configure(EntityTypeBuilder<Medication> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("Medications");

            // Properties
            builder.Property(m => m.BrandName).IsRequired().HasMaxLength(250);
            builder.Property(m => m.GenericName).IsRequired(false).HasMaxLength(200);
            builder.Property(m => m.Strength).IsRequired(false).HasMaxLength(100);
            builder.Property(m => m.DosageForm).IsRequired().HasConversion<int>();

            // Indexes
            builder.HasIndex(m => m.BrandName)
                .HasDatabaseName("IX_Medication_BrandName");

            builder.HasIndex(m => m.GenericName)
                .HasDatabaseName("IX_Medication_GenericName");

            builder.HasIndex(m => m.DosageForm)
                .HasDatabaseName("IX_Medication_DosageForm");

            // Prescribed Medications Relationship (One-to-Many)
            builder.HasMany(m => m.PrescribedMedications)
                .WithOne(pm => pm.Medication)
                .HasForeignKey(pm => pm.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
