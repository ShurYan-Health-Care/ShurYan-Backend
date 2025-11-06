using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations.LaboratoryConfigurations
{
    public class LabTestEntityConfiguration : AuditableEntityConfiguration<LabTest>
	{
		public override void Configure(EntityTypeBuilder<LabTest> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("LabTests");

			// Properties
			builder.Property(lt => lt.Name).IsRequired().HasMaxLength(200);
			builder.Property(lt => lt.Code).IsRequired().HasMaxLength(50);
			builder.Property(lt => lt.Category).IsRequired().HasConversion<int>();
			builder.Property(lt => lt.SpecialInstructions).IsRequired(false).HasMaxLength(500);

			// Indexes
			builder.HasIndex(lt => lt.Code)
				.IsUnique()
				.HasDatabaseName("IX_LabTest_Code");

			builder.HasIndex(lt => lt.Name)
				.HasDatabaseName("IX_LabTest_Name");

			builder.HasIndex(lt => lt.Category)
				.HasDatabaseName("IX_LabTest_Category");

			// Lab Services Relationship (One-to-Many)
			builder.HasMany(lt => lt.LabServices)
				.WithOne(ls => ls.LabTest)
				.HasForeignKey(ls => ls.LabTestId)
				.OnDelete(DeleteBehavior.Restrict);

			// Prescription Items Relationship (One-to-Many)
			builder.HasMany(lt => lt.PrescriptionItems)
				.WithOne(lpi => lpi.LabTest)
				.HasForeignKey(lpi => lpi.LabTestId)
				.OnDelete(DeleteBehavior.Restrict);

			// Lab Results Relationship (One-to-Many)
			builder.HasMany(lt => lt.LabResults)
				.WithOne(lr => lr.LabTest)
				.HasForeignKey(lr => lr.LabTestId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}