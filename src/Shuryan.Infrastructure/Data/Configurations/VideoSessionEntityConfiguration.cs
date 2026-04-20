using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class VideoSessionEntityConfiguration : AuditableEntityConfiguration<VideoSession>
    {
        public override void Configure(EntityTypeBuilder<VideoSession> builder)
        {
            base.Configure(builder);

            builder.ToTable("VideoSessions");

            builder.Property(vs => vs.AppointmentId).IsRequired();
            builder.Property(vs => vs.DoctorId).IsRequired();
            builder.Property(vs => vs.PatientId).IsRequired();
            builder.Property(vs => vs.AgoraChannelName).IsRequired().HasMaxLength(100);
            builder.Property(vs => vs.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(VideoSessionStatus.Waiting);
            builder.Property(vs => vs.DoctorJoinedAt).IsRequired(false);
            builder.Property(vs => vs.PatientJoinedAt).IsRequired(false);
            builder.Property(vs => vs.StartedAt).IsRequired(false);
            builder.Property(vs => vs.EndedAt).IsRequired(false);
            builder.Property(vs => vs.DurationSeconds).IsRequired(false);
            builder.Property(vs => vs.EndReason).IsRequired(false).HasConversion<int>();

            builder.HasIndex(vs => vs.AppointmentId)
                .IsUnique()
                .HasDatabaseName("IX_VideoSession_AppointmentId");

            builder.HasIndex(vs => vs.DoctorId)
                .HasDatabaseName("IX_VideoSession_DoctorId");

            builder.HasIndex(vs => vs.PatientId)
                .HasDatabaseName("IX_VideoSession_PatientId");

            builder.HasIndex(vs => vs.Status)
                .HasDatabaseName("IX_VideoSession_Status");

            builder.HasOne(vs => vs.Appointment)
                .WithOne(a => a.VideoSession)
                .HasForeignKey<VideoSession>(vs => vs.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(vs => vs.Doctor)
                .WithMany()
                .HasForeignKey(vs => vs.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(vs => vs.Patient)
                .WithMany()
                .HasForeignKey(vs => vs.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasCheckConstraint(
                "CK_VideoSession_Duration",
                "[DurationSeconds] IS NULL OR [DurationSeconds] >= 0");
        }
    }
}
