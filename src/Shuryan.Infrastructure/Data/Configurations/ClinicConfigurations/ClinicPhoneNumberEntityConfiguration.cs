using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Common;

namespace Shuryan.Infrastructure.Data.Configurations.ClinicConfigurations
{
    public class ClinicPhoneNumberEntityConfiguration : IEntityTypeConfiguration<ClinicPhoneNumber>
    {
        public void Configure(EntityTypeBuilder<ClinicPhoneNumber> builder)
        {
            builder.HasKey(cpn => cpn.Id);

            builder.Property(cpn => cpn.Number)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(cpn => cpn.Type)
                   .HasConversion<int>()
                   .IsRequired();

            // Relationships
            builder.HasOne(cpn => cpn.Clinic)
                   .WithMany(c => c.PhoneNumbers)
                   .HasForeignKey(cpn => cpn.ClinicId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
