using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical;

namespace Shuryan.Infrastructure.Data.Configurations.DoctorConfigurations
{
    public class DoctorServiceEntityConfiguration : IEntityTypeConfiguration<DoctorService>
    {
        public void Configure(EntityTypeBuilder<DoctorService> builder)
        {
            builder.HasKey(ds => ds.Id);

            builder.Property(ds => ds.ConsultationType)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(ds => ds.ConsultationFee)
                   .HasPrecision(10, 2)
                   .IsRequired();

            builder.Property(ds => ds.SessionDurationMinutes)
                   .IsRequired();

            // Relationships
            builder.HasOne(ds => ds.Doctor)
                   .WithMany(d => d.Services)
                   .HasForeignKey(ds => ds.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Constraints
            builder.HasCheckConstraint("CK_DoctorService_ConsultationFee", "[ConsultationFee] > 0");

            builder.HasCheckConstraint("CK_DoctorService_SessionDuration", "[SessionDurationMinutes] > 0 AND [SessionDurationMinutes] <= 480");
        }
    }
}
