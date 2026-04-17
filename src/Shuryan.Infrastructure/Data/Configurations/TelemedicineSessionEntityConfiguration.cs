using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class TelemedicineSessionEntityConfiguration : AuditableEntityConfiguration<TelemedicineSession>
    {
        public override void Configure(EntityTypeBuilder<TelemedicineSession> builder)
        {
            base.Configure(builder);

            // Table Mapping
            builder.ToTable("TelemedicineSessions");

            // Properties
            builder.Property(ts => ts.AppointmentId).IsRequired();
            builder.Property(ts => ts.DoctorId).IsRequired();
            builder.Property(ts => ts.PatientId).IsRequired();
            builder.Property(ts => ts.RoomId).IsRequired().HasMaxLength(100);
            builder.Property(ts => ts.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(TelemedicineSessionStatus.Waiting);
            builder.Property(ts => ts.DoctorConnectionId).IsRequired(false).HasMaxLength(256);
            builder.Property(ts => ts.PatientConnectionId).IsRequired(false).HasMaxLength(256);
            builder.Property(ts => ts.DoctorJoinedAt).IsRequired(false);
            builder.Property(ts => ts.PatientJoinedAt).IsRequired(false);
            builder.Property(ts => ts.StartedAt).IsRequired(false);
            builder.Property(ts => ts.EndedAt).IsRequired(false);
            builder.Property(ts => ts.DurationSeconds).IsRequired(false);
            builder.Property(ts => ts.EndReason).IsRequired(false).HasConversion<int>();

            // Indexes
            builder.HasIndex(ts => ts.AppointmentId)
                .IsUnique()
                .HasDatabaseName("IX_TelemedicineSession_AppointmentId");

            builder.HasIndex(ts => ts.DoctorId)
                .HasDatabaseName("IX_TelemedicineSession_DoctorId");

            builder.HasIndex(ts => ts.PatientId)
                .HasDatabaseName("IX_TelemedicineSession_PatientId");

            builder.HasIndex(ts => ts.Status)
                .HasDatabaseName("IX_TelemedicineSession_Status");

            builder.HasIndex(ts => ts.RoomId)
                .IsUnique()
                .HasDatabaseName("IX_TelemedicineSession_RoomId");

            // Relationships
            builder.HasOne(ts => ts.Appointment)
                .WithOne(a => a.TelemedicineSession)
                .HasForeignKey<TelemedicineSession>(ts => ts.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ts => ts.Doctor)
                .WithMany()
                .HasForeignKey(ts => ts.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ts => ts.Patient)
                .WithMany()
                .HasForeignKey(ts => ts.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Check Constraints
            builder.HasCheckConstraint(
                "CK_TelemedicineSession_Duration",
                "[DurationSeconds] IS NULL OR [DurationSeconds] >= 0");
        }
    }
}
