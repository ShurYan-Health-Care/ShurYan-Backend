using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.System;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class EmergencyAuditLogConfiguration : IEntityTypeConfiguration<EmergencyAuditLog>
    {
        public void Configure(EntityTypeBuilder<EmergencyAuditLog> builder)
        {
            builder.HasOne(e => e.Doctor)
                   .WithMany()
                   .HasForeignKey(e => e.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Patient)
                   .WithMany()
                   .HasForeignKey(e => e.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
