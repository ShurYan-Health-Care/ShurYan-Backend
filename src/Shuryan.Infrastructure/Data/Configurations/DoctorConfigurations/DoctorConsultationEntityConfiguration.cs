using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Consultations;

namespace Shuryan.Infrastructure.Data.Configurations.DoctorConfigurations
{
    public class DoctorConsultationEntityConfiguration : IEntityTypeConfiguration<DoctorConsultation>
    {
        public void Configure(EntityTypeBuilder<DoctorConsultation> builder)
        {
            // Table Mapping
            builder.ToTable("DoctorConsultations");

            // Composite Primary Key (DoctorId + ConsultationTypeId)
            builder.HasKey(dc => new { dc.DoctorId, dc.ConsultationTypeId });

            // Properties
            builder.Property(dc => dc.DoctorId).IsRequired();
            builder.Property(dc => dc.ConsultationTypeId).IsRequired();
            builder.Property(dc => dc.ConsultationFee).IsRequired().HasPrecision(10, 2);
            builder.Property(dc => dc.SessionDurationMinutes).IsRequired();

            // Ignore Id from AuditableEntity (using Composite Key instead)
            builder.Ignore(dc => dc.Id);

            // Doctor Relationship (Many-to-One)
            builder.HasOne(dc => dc.Doctor)
                .WithMany(d => d.Consultations)
                .HasForeignKey(dc => dc.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // ConsultationType Relationship (Many-to-One)
            builder.HasOne(dc => dc.ConsultationType)
                .WithMany(ct => ct.Consultations)
                .HasForeignKey(dc => dc.ConsultationTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check Constraints
            builder.HasCheckConstraint("CK_DoctorConsultation_Fee", "[ConsultationFee] >= 0");
            builder.HasCheckConstraint("CK_DoctorConsultation_Duration", "[SessionDurationMinutes] >= 15 AND [SessionDurationMinutes] <= 120");
        }
    }
}
