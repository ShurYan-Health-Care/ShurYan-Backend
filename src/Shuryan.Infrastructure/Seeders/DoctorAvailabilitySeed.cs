using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Enums;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class DoctorAvailabilitySeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.DoctorAvailability.AnyAsync())
            {
                Console.WriteLine("Doctor Availabilities already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Doctor Availabilities...");

            var doctors = await context.Doctors.ToListAsync();
            var availabilities = new List<DoctorAvailability>();

            foreach (var doctor in doctors)
            {
                // Saturday to Thursday: 10:00 AM - 4:00 PM
                for (int day = 1; day <= 5; day++)
                {
                    availabilities.Add(new DoctorAvailability
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = doctor.Id,
                        DayOfWeek = (SysDayOfWeek)day,
                        StartTime = new TimeOnly(10, 0),
                        EndTime = new TimeOnly(16, 0),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Friday: Shorter hours
                availabilities.Add(new DoctorAvailability
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctor.Id,
                    DayOfWeek = SysDayOfWeek.Friday,
                    StartTime = new TimeOnly(14, 0),
                    EndTime = new TimeOnly(18, 0),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.DoctorAvailability.AddRangeAsync(availabilities);
            await context.SaveChangesAsync();

            Console.WriteLine($"{availabilities.Count} Doctor Availabilities seeded successfully!");
        }
    }
}