using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Medical;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
	public class LabTestEntityConfiguration : IEntityTypeConfiguration<LabTest>
	{
		public void Configure(EntityTypeBuilder<LabTest> builder)
		{
			builder.HasKey(lt => lt.Id);

			builder.Property(lt => lt.Name)
				   .IsRequired()
				   .HasMaxLength(200);

			builder.Property(lt => lt.Code)
				   .IsRequired()
				   .HasMaxLength(50);

			builder.Property(lt => lt.Category)
				   .HasConversion<int>()
				   .IsRequired();

			builder.Property(lt => lt.SpecialInstructions)
				   .HasMaxLength(500);

			builder.Property(lt => lt.IsActive)
				   .IsRequired()
				   .HasDefaultValue(true);

			builder.Property(lt => lt.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

			// Relationships
			builder.HasMany(lt => lt.LabServices)
				   .WithOne(ls => ls.LabTest)
				   .HasForeignKey(ls => ls.LabTestId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(lt => lt.PrescriptionItems)
				   .WithOne(lpi => lpi.LabTest)
				   .HasForeignKey(lpi => lpi.LabTestId)
				   .OnDelete(DeleteBehavior.Restrict);
		}
	}
}