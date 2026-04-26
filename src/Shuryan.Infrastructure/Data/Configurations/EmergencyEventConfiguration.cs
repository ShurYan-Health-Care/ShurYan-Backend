using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Medical;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class EmergencyEventConfiguration : IEntityTypeConfiguration<EmergencyEvent>
    {
        public void Configure(EntityTypeBuilder<EmergencyEvent> builder)
        {
            builder.HasOne(e => e.Patient)
                   .WithMany()
                   .HasForeignKey(e => e.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ActivatingDoctor)
                   .WithMany()
                   .HasForeignKey(e => e.ActivatingDoctorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
