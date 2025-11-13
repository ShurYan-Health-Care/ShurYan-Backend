using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class LabPrescriptionSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.LabPrescriptions.AnyAsync())
            {
                Console.WriteLine("Lab Prescriptions already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Lab Prescriptions...");

            var completedAppointments = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .Take(8)
                .ToListAsync();

            var labTests = await context.LabTests.ToListAsync();
            var labPrescriptions = new List<LabPrescription>();
            var prescriptionItems = new List<LabPrescriptionItem>();

            var random = new Random();

            foreach (var appointment in completedAppointments)
            {
                var labPrescription = new LabPrescription
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    DoctorId = appointment.DoctorId,
                    PatientId = appointment.PatientId,
                    GeneralNotes = "إجراء التحاليل على معدة فارغة إذا كان ذلك مطلوباً",
                    CreatedAt = appointment.ScheduledStartTime.AddHours(1)
                };
                labPrescriptions.Add(labPrescription);

                // Add 2-5 lab tests per prescription
                var testCount = random.Next(2, 6);
                var selectedTests = labTests.OrderBy(x => random.Next()).Take(testCount).ToList();

                foreach (var test in selectedTests)
                {
                    prescriptionItems.Add(new LabPrescriptionItem
                    {
                        Id = Guid.NewGuid(),
                        LabPrescriptionId = labPrescription.Id,
                        LabTestId = test.Id,
                        DoctorNotes = test.SpecialInstructions,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.LabPrescriptions.AddRangeAsync(labPrescriptions);
            await context.LabPrescriptionItems.AddRangeAsync(prescriptionItems);
            await context.SaveChangesAsync();

            Console.WriteLine($"{labPrescriptions.Count} Lab Prescriptions and {prescriptionItems.Count} Items seeded!");
        }
    }
}