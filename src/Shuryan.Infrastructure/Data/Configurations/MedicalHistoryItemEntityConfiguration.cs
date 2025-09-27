using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;

namespace Shuryan.Infrastructure.Data.Configurations
{
	public class MedicalHistoryItemEntityConfiguration : IEntityTypeConfiguration<MedicalHistoryItem>
	{
		public void Configure(EntityTypeBuilder<MedicalHistoryItem> builder)
		{
			builder.HasKey(mhi => mhi.Id);

			builder.Property(mhi => mhi.Type)
				   .HasConversion<int>()
				   .IsRequired();

			builder.Property(mhi => mhi.Text)
				   .IsRequired()
				   .HasMaxLength(1000);

			// Relationships
			builder.HasOne(mhi => mhi.Patient)
				   .WithMany(p => p.MedicalHistory)
				   .HasForeignKey(mhi => mhi.PatientId)
				   .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
