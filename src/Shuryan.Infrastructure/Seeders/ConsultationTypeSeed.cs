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
    public static class ConsultationTypeSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.ConsultationTypes.AnyAsync())
            {
                Console.WriteLine("Consultation Types already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Consultation Types...");

            var consultationTypes = new List<ConsultationType>
            {
                new ConsultationType
                {
                    Id = Guid.NewGuid(),
                    ConsultationTypeEnum = ConsultationTypeEnum.Regular
                },
                new ConsultationType
                {
                    Id = Guid.NewGuid(),
                    ConsultationTypeEnum = ConsultationTypeEnum.FollowUp
                }
            };

            await context.ConsultationTypes.AddRangeAsync(consultationTypes);
            await context.SaveChangesAsync();

            Console.WriteLine($"{consultationTypes.Count} Consultation Types seeded successfully!");
        }
    }
}