using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.External.Payments;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Entities.Base;
using System.Linq.Expressions;
using Shuryan.Core.Entities.Medical;

namespace Shuryan.Infrastructure.Data
{
	public class ShuryanDbContext : IdentityDbContext<User, Role, Guid>
	{

		public ShuryanDbContext(DbContextOptions<ShuryanDbContext> options)
		: base(options)
		{
		}

		#region DbSets
		/// <summary>
		/// External Entities
		/// </summary>
		// Clinics
		public DbSet<Clinic> Clinics { get; set; }
		public DbSet<ClinicPhoneNumber> ClinicPhoneNumbers { get; set; }
		public DbSet<ClinicPhoto> ClinicPhotos { get; set; }
		public DbSet<ClinicService> ClinicServices { get; set; }
		// Laboratories
		public DbSet<LabOrder> LabOrders { get; set; }
		public DbSet<LabPrescription> LabPrescriptions { get; set; }
		public DbSet<LabPrescriptionItem> LabPrescriptionItems { get; set; }
		public DbSet<LabResult> LabResults { get; set; }
		public DbSet<LabService> LabServices { get; set; }
		public DbSet<LabTest> LabTests { get; set; }
		public DbSet<LabWorkingHours> LabWorkingHours { get; set; }
		// Pharmacies
		public DbSet<Medication> Medications { get; set; }
		public DbSet<PharmacyOrder> PharmacyOrders { get; set; }
		public DbSet<PharmacyOrderItem> PharmacyOrderItems { get; set; }
		public DbSet<PharmacyWorkingHours> PharmacyWorkingHours { get; set; }
		public DbSet<PrescribedMedication> PrescribedMedications { get; set; }
		public DbSet<Prescription> Prescriptions { get; set; }
		public DbSet<DispensingRecord> DispensingRecords { get; set; }
		public DbSet<DispensedMedicationItem> DispensedMedicationItems { get; set; }

		/// <summary>
		/// Identity Entities
		/// </summary>
		public DbSet<Doctor> Doctors { get; set; }
		public DbSet<Laboratory> Laboratories { get; set; }
		public DbSet<Patient> Patients { get; set; }
		public DbSet<Pharmacy> Pharmacies { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Verifier> Verifiers { get; set; }
		public DbSet<RefreshToken> RefreshTokens { get; set; }

		/// <summary>
		/// Medical Entities
		/// </summary>
		// Appointments
		public DbSet<Appointment> Appointments { get; set; }
		public DbSet<ConsultationRecord> ConsultationRecords { get; set; }
		// Consultations
		public DbSet<ConsultationType> ConsultationTypes { get; set; }
		public DbSet<DoctorConsultation> DoctorConsultations { get; set; }
		// Schedules
		public DbSet<DoctorAvailability> DoctorAvailability { get; set; }
		public DbSet<DoctorOverride> DoctorOverride { get; set; }

		/// <summary>
		/// Shared Entities
		/// </summary>
		public DbSet<Address> Addresses { get; set; }
		public DbSet<DoctorDocument> DoctorDocument { get; set; }
		public DbSet<LaboratoryDocument> LaboratoryDocuments { get; set; }
		public DbSet<MedicalHistoryItem> MedicalHistoryItems { get; set; }
		public DbSet<PharmacyDocument> PharmacyDocuments { get; set; }

		/// <summary>
		/// System Entities
		/// </summary>
		// Review
		public DbSet<DoctorReview> DoctorReviews { get; set; }
		public DbSet<LaboratoryReview> LaboratoryReviews { get; set; }
		public DbSet<PharmacyReview> PharmacyReviews { get; set; }
		// Notification
		public DbSet<Notification> Notifications { get; set; }
		// Email Verification
		public DbSet<EmailVerification> EmailVerifications { get; set; }
		// Chat/AI Bot
		public DbSet<Conversation> Conversations { get; set; }
		public DbSet<ConversationMessage> ConversationMessages { get; set; }
		// Payment
		public DbSet<Payment> Payments { get; set; }
		public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
		#endregion

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.ApplyConfigurationsFromAssembly(typeof(ShuryanDbContext).Assembly);
		}

	}
}
