using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class DoctorConsultationSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.DoctorConsultations.AnyAsync())
            {
                Console.WriteLine("Doctor Consultations already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Doctor Consultations...");

            var doctors = await context.Doctors.ToListAsync();
            var consultationTypes = await context.ConsultationTypes.ToListAsync();
            var consultations = new List<DoctorConsultation>();

            foreach (var doctor in doctors)
            {
                foreach (var consultationType in consultationTypes)
                {
                    var fee = consultationType.ConsultationTypeEnum == ConsultationTypeEnum.Regular
                        ? 200m + (doctor.YearsOfExperience * 10m)
                        : 100m + (doctor.YearsOfExperience * 5m);

                    consultations.Add(new DoctorConsultation
                    {
                        DoctorId = doctor.Id,
                        ConsultationTypeId = consultationType.Id,
                        ConsultationFee = fee,
                        SessionDurationMinutes = 30
                    });
                }
            }

            await context.DoctorConsultations.AddRangeAsync(consultations);
            await context.SaveChangesAsync();

            Console.WriteLine($"{consultations.Count} Doctor Consultations seeded successfully!");
        }
    }
}