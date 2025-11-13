using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class PrescriptionSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Prescriptions.AnyAsync())
            {
                Console.WriteLine("Prescriptions already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Prescriptions...");

            var completedAppointments = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .Take(10)
                .ToListAsync();

            var medications = await context.Medications.ToListAsync();
            var prescriptions = new List<Prescription>();
            var prescribedMeds = new List<PrescribedMedication>();

            var random = new Random();

            foreach (var appointment in completedAppointments)
            {
                var prescription = new Prescription
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    DoctorId = appointment.DoctorId,
                    PatientId = appointment.PatientId,
                    PrescriptionNumber = $"RX-{DateTime.UtcNow.Year}-{random.Next(10000, 99999)}",
                    DigitalSignature = $"DR-{appointment.DoctorId.ToString().Substring(0, 8)}",
                    GeneralInstructions = "تناول الدواء بعد الأكل مع كمية كافية من الماء",
                    CreatedAt = appointment.ScheduledStartTime.AddHours(1)
                };
                prescriptions.Add(prescription);

                // Add 2-4 medications per prescription
                var medCount = random.Next(2, 5);
                var selectedMeds = medications.OrderBy(x => random.Next()).Take(medCount).ToList();

                foreach (var med in selectedMeds)
                {
                    prescribedMeds.Add(new PrescribedMedication
                    {
                        MedicationPrescriptionId = prescription.Id,
                        MedicationId = med.Id,
                        Dosage = "قرص واحد",
                        Frequency = random.Next(2) == 0 ? "3 مرات يومياً" : "مرتين يومياً",
                        DurationDays = random.Next(5, 15),
                        SpecialInstructions = "بعد الأكل"
                    });
                }
            }

            await context.Prescriptions.AddRangeAsync(prescriptions);
            await context.PrescribedMedications.AddRangeAsync(prescribedMeds);
            await context.SaveChangesAsync();

            Console.WriteLine($"{prescriptions.Count} Prescriptions and {prescribedMeds.Count} Prescribed Medications seeded!");
        }
    }
}