using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Identity;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class VerifierSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Verifiers.AnyAsync())
            {
                Console.WriteLine("Verifiers already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Verifiers...");

            var adminId = Guid.NewGuid(); // Simulated admin ID

            var verifiers = new List<Verifier>
            {
                new Verifier
                {
                    Id = Guid.NewGuid(),
                    FirstName = "أحمد",
                    LastName = "المراجع",
                    Email = "ahmed.verifier@shuryan.com",
                    UserName = "ahmed.verifier@shuryan.com",
                    PhoneNumber = "+201001234567",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByAdminId = adminId
                },
                new Verifier
                {
                    Id = Guid.NewGuid(),
                    FirstName = "فاطمة",
                    LastName = "السيد",
                    Email = "fatma.verifier@shuryan.com",
                    UserName = "fatma.verifier@shuryan.com",
                    PhoneNumber = "+201002345678",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByAdminId = adminId
                },
                new Verifier
                {
                    Id = Guid.NewGuid(),
                    FirstName = "محمد",
                    LastName = "حسن",
                    Email = "mohamed.verifier@shuryan.com",
                    UserName = "mohamed.verifier@shuryan.com",
                    PhoneNumber = "+201003456789",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByAdminId = adminId
                }
            };

            await context.Verifiers.AddRangeAsync(verifiers);
            await context.SaveChangesAsync();

            Console.WriteLine($"{verifiers.Count} Verifiers seeded successfully!");
        }
    }
}