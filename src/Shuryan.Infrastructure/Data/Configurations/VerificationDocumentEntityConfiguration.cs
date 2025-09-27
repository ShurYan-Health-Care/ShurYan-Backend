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
	public class VerificationDocumentEntityConfiguration : IEntityTypeConfiguration<VerificationDocument>
	{
		public void Configure(EntityTypeBuilder<VerificationDocument> builder)
		{
			builder.HasKey(vd => vd.Id);

			builder.Property(vd => vd.Type)
				   .HasConversion<int>()
				   .IsRequired();

			// Relationships
			builder.HasOne(vd => vd.Doctor)
				   .WithMany(d => d.VerificationDocuments)
				   .HasForeignKey(vd => vd.DoctorId)
				   .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
