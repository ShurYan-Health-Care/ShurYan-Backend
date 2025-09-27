using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Entities.System;

namespace Shuryan.Infrastructure.Data
{
	public class ShuryanDbContext : IdentityDbContext<User, Role, Guid>
	{
		public ShuryanDbContext(DbContextOptions<ShuryanDbContext> options)
		: base(options)
		{
		}
		#region DbSets
		// Identity Entities
		public DbSet<User> Users { get; set; }

		// System Entities
		public DbSet<Clinic> Clinics { get; set; }

		// Common Entities
		public DbSet<Address> Addresses { get; set; }
		public DbSet<ClinicPhoto> ClinicPhotos { get; set; }
		public DbSet<ClinicPhoneNumber> ClinicPhoneNumbers { get; set; }
		public DbSet<MedicalHistoryItem> MedicalHistoryItems { get; set; }
		public DbSet<VerificationDocument> VerificationDocuments { get; set; }
		public DbSet<ClinicService> ClinicServices { get; set; }

		// Medical Entities
		public DbSet<DoctorService> DoctorService { get; set; }
		public DbSet<DoctorOverride> DoctorOverride { get; set; }
		public DbSet<DoctorAvailability> DoctorAvailability { get; set; }
		#endregion

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.ApplyConfigurationsFromAssembly(typeof(ShuryanDbContext).Assembly);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);


		}
	}
}
