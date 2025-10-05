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
    public class ConsultationTypeEntityConfiguration : IEntityTypeConfiguration<ConsultationType>
	{
		public void Configure(EntityTypeBuilder<ConsultationType> builder)
		{
			builder.HasKey(ct => ct.Id);

			builder.Property(ct => ct.ConsultationTypeEnum)
				   .IsRequired()
				   .HasConversion<int>();

			builder.HasMany(ct => ct.Consultations)
				   .WithOne(dc => dc.ConsultationType)
				   .HasForeignKey(dc => dc.ConsultationTypeId)
				   .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
