using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External;
using Shuryan.Core.Entities.Identity;
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
        // Identity Entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Verifier> Verifiers { get; set; }
        public DbSet<Laboratory> Laboratories { get; set; }

        // External Entities
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<PharmacyDocument> PharmacyDocuments { get; set; }
        public DbSet<PharmacyOrder> PharmacyOrders { get; set; }
        public DbSet<PharmacyWorkingHours> PharmacyWorkingHours { get; set; }
        public DbSet<PrescribedMedication> PrescribedMedications { get; set; }
        public DbSet<Prescription> Prescription { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }

        // Common Entities
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ClinicPhoto> ClinicPhotos { get; set; }
        public DbSet<ClinicPhoneNumber> ClinicPhoneNumbers { get; set; }
        public DbSet<MedicalHistoryItem> MedicalHistoryItems { get; set; }
        public DbSet<VerificationDocument> VerificationDocuments { get; set; }
        public DbSet<ClinicService> ClinicServices { get; set; }
        public DbSet<LaboratoryDocument> LaboratoryDocuments { get; set; }

        // Medical Entities
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ConsultationRecord> ConsultationRecords { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailability { get; set; }
        public DbSet<DoctorOverride> DoctorOverride { get; set; }
        public DbSet<DoctorService> DoctorService { get; set; }
        public DbSet<LaboratoryWorkingHours> LaboratoryWorkingHours { get; set; }
        public DbSet<LabOrder> LabOrders { get; set; }
        public DbSet<LabPrescription> LabPrescriptions { get; set; }
        public DbSet<LabPrescriptionItem> LabPrescriptionItems { get; set; }
        public DbSet<LabResult> LabResults { get; set; }
        public DbSet<LabService> LabServices { get; set; }
        public DbSet<LabTest> LabTests { get; set; }
        #endregion


        // --- DbSets for Appointment Flow ---
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ShuryanDbContext).Assembly);
        }
    }
}
