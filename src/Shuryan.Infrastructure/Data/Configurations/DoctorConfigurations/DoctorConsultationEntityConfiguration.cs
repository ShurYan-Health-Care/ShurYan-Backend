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
			// Composite Key (DoctorId + ConsultationTypeId)
			builder.HasKey(dc => new { dc.DoctorId, dc.ConsultationTypeId });

			builder.Property(dc => dc.ConsultationFee)
				   .IsRequired()
				   .HasColumnType("decimal(18,2)");

			builder.Property(dc => dc.SessionDurationMinutes)
				   .IsRequired();

			builder.HasOne(dc => dc.Doctor)
				   .WithMany(d => d.Consultations)
				   .HasForeignKey(dc => dc.DoctorId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(dc => dc.ConsultationType)
				   .WithMany(ct => ct.Consultations)
				   .HasForeignKey(dc => dc.ConsultationTypeId)
				   .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
