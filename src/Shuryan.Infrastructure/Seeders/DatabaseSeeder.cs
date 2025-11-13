using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ShuryanDbContext>();

            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed in correct order (respecting foreign key dependencies)
            Console.WriteLine("Starting database seeding...");

            // 1. Seed Base Data (No FK dependencies)
            await ConsultationTypeSeed.SeedAsync(context);
            await LabTestSeed.SeedAsync(context);
            await MedicationSeed.SeedAsync(context);

            // 2. Seed Users (Identity)
            await VerifierSeed.SeedAsync(context);
            await PatientSeed.SeedAsync(context);
            await DoctorSeed.SeedAsync(context);
            await LaboratorySeed.SeedAsync(context);
            await PharmacySeed.SeedAsync(context);

            // 3. Seed Related Entities
            await DoctorAvailabilitySeed.SeedAsync(context);
            await DoctorConsultationSeed.SeedAsync(context);
            await ClinicSeed.SeedAsync(context);
            await LabServiceSeed.SeedAsync(context);

            // 4. Seed Transactional Data
            await AppointmentSeed.SeedAsync(context);
            await PrescriptionSeed.SeedAsync(context);
            await LabPrescriptionSeed.SeedAsync(context);

            // 5. Seed Reviews (depends on completed appointments)
            await ReviewSeed.SeedAsync(context);

            Console.WriteLine("Database seeding completed successfully!");
        }

        public static async Task ClearDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ShuryanDbContext>();

            Console.WriteLine("🗑️ Clearing database...");

            // Clear in reverse order of seeding (respecting FK dependencies)
            
            // 1. Reviews (depends on appointments, doctors, labs, pharmacies)
            context.DoctorReviews.RemoveRange(context.DoctorReviews);
            context.LaboratoryReviews.RemoveRange(context.LaboratoryReviews);
            context.PharmacyReviews.RemoveRange(context.PharmacyReviews);

            // 2. Transactional Data - child tables first
            context.LabPrescriptionItems.RemoveRange(context.LabPrescriptionItems);
            context.LabPrescriptions.RemoveRange(context.LabPrescriptions);
            context.PrescribedMedications.RemoveRange(context.PrescribedMedications);
            context.Prescriptions.RemoveRange(context.Prescriptions);
            context.ConsultationRecords.RemoveRange(context.ConsultationRecords);
            context.Appointments.RemoveRange(context.Appointments);
            context.LabResults.RemoveRange(context.LabResults);
            context.PharmacyOrders.RemoveRange(context.PharmacyOrders);
            context.LabOrders.RemoveRange(context.LabOrders);

            // 3. Related Entities - child tables first
            context.LabServices.RemoveRange(context.LabServices);
            context.LabWorkingHours.RemoveRange(context.LabWorkingHours);
            context.PharmacyWorkingHours.RemoveRange(context.PharmacyWorkingHours);
            context.ClinicServices.RemoveRange(context.ClinicServices);
            context.ClinicPhoneNumbers.RemoveRange(context.ClinicPhoneNumbers);
            context.ClinicPhotos.RemoveRange(context.ClinicPhotos);
            context.Clinics.RemoveRange(context.Clinics);
            context.DoctorConsultations.RemoveRange(context.DoctorConsultations);
            context.DoctorAvailability.RemoveRange(context.DoctorAvailability);
            context.DoctorOverride.RemoveRange(context.DoctorOverride);
            
            // 4. Documents
            context.DoctorDocument.RemoveRange(context.DoctorDocument);
            context.LaboratoryDocuments.RemoveRange(context.LaboratoryDocuments);
            context.PharmacyDocuments.RemoveRange(context.PharmacyDocuments);
            
            // 5. Medical History
            context.MedicalHistoryItems.RemoveRange(context.MedicalHistoryItems);

            // 6. Users/Identity
            context.Pharmacies.RemoveRange(context.Pharmacies);
            context.Laboratories.RemoveRange(context.Laboratories);
            context.Doctors.RemoveRange(context.Doctors);
            context.Patients.RemoveRange(context.Patients);
            context.Verifiers.RemoveRange(context.Verifiers);

            // 7. Addresses
            context.Addresses.RemoveRange(context.Addresses);

            // 8. Base/Master Data
            context.Medications.RemoveRange(context.Medications);
            context.LabTests.RemoveRange(context.LabTests);
            context.ConsultationTypes.RemoveRange(context.ConsultationTypes);

            await context.SaveChangesAsync();
            Console.WriteLine("Database cleared successfully!");
        }
    }
}