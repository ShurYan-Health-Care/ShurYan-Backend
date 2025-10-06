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

            builder.HasKey(m => m.Id);

            builder.Property(m => m.BrandName)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(m => m.GenericName)
                .HasMaxLength(200);

            builder.Property(m => m.Strength)
                .HasMaxLength(100);

            builder.Property(m => m.DosageForm)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasMany(m => m.PrescribedMedications)
                .WithOne(pm => pm.Medication)
                .HasForeignKey(pm => pm.MedicationId);

        }
    }
}
