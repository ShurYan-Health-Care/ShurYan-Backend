using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Infrastructure.Data.Configurations
{
	public class PatientEntityConfiguration : IEntityTypeConfiguration<Patient>
	{
		public void Configure(EntityTypeBuilder<Patient> builder)
		{
			// Relationships
			builder.HasOne(p => p.Address)
				   .WithOne()
				   .HasForeignKey<Patient>(p => p.AddressId)
				   .OnDelete(DeleteBehavior.SetNull);

			builder.HasMany(p => p.MedicalHistory)
				   .WithOne(mhi => mhi.Patient)
				   .HasForeignKey(mhi => mhi.PatientId)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(p => p.Appointments)
				   .WithOne(a => a.Patient)
				   .HasForeignKey(a => a.PatientId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(p => p.LabOrders)
				   .WithOne(lo => lo.Patient)
				   .HasForeignKey(lo => lo.PatientId)
				   .OnDelete(DeleteBehavior.Restrict);
		}
	}
}
