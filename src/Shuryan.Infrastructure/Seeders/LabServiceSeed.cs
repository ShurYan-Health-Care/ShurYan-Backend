using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class LabServiceSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.LabServices.AnyAsync())
            {
                Console.WriteLine("Lab Services already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Lab Services...");

            var laboratories = await context.Laboratories.ToListAsync();
            var labTests = await context.LabTests.ToListAsync();
            var labServices = new List<LabService>();

            var random = new Random();

            foreach (var lab in laboratories)
            {
                // Each lab offers 15-20 random tests
                var selectedTests = labTests.OrderBy(x => random.Next()).Take(random.Next(15, 21)).ToList();

                foreach (var test in selectedTests)
                {
                    var basePrice = random.Next(50, 500);

                    labServices.Add(new LabService
                    {
                        Id = Guid.NewGuid(),
                        LaboratoryId = lab.Id,
                        LabTestId = test.Id,
                        Price = basePrice,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.LabServices.AddRangeAsync(labServices);
            await context.SaveChangesAsync();

            Console.WriteLine($"{labServices.Count} Lab Services seeded successfully!");
        }
    }
}