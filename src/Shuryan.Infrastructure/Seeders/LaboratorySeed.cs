using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums;
using Shuryan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Shared;

namespace Shuryan.Infrastructure.Seeders
{
    public static class LaboratorySeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Laboratories.AnyAsync())
            {
                Console.WriteLine("Laboratories already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Laboratories...");

            var verifier = await context.Verifiers.FirstOrDefaultAsync();
            if (verifier == null)
            {
                Console.WriteLine(" No verifier found. Please seed verifiers first.");
                return;
            }

            // Create Addresses for Laboratories
            var labAddresses = new List<Address>
            {
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الجلاء",
                    City = "الزقازيق",
                    Governorate = Governorate.Sharqia,
                    BuildingNumber = "15",
                    Latitude = 30.5925,
                    Longitude = 31.5033,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع رمسيس",
                    City = "القاهرة",
                    Governorate = Governorate.Cairo,
                    BuildingNumber = "88",
                    Latitude = 30.0626,
                    Longitude = 31.2497,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع فيصل",
                    City = "الجيزة",
                    Governorate = Governorate.Giza,
                    BuildingNumber = "120",
                    Latitude = 30.0131,
                    Longitude = 31.2089,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "طريق الحرية",
                    City = "الإسكندرية",
                    Governorate = Governorate.Alexandria,
                    BuildingNumber = "55",
                    Latitude = 31.2156,
                    Longitude = 29.9553,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الجيش",
                    City = "المنصورة",
                    Governorate = Governorate.Dakahlia,
                    BuildingNumber = "42",
                    Latitude = 31.0364,
                    Longitude = 31.3807,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Addresses.AddRangeAsync(labAddresses);
            await context.SaveChangesAsync();

            var laboratories = new List<Laboratory>
            {
                new Laboratory
                {
                    Id = Guid.NewGuid(),
                    Name = "معامل المختبر",
                    Description = "معمل تحاليل طبية شامل مع أحدث الأجهزة والتقنيات. نقدم جميع أنواع التحاليل الطبية بدقة عالية.",
                    Email = "info@almokhtaber.com",
                    UserName = "info@almokhtaber.com",
                    PhoneNumber = "+201711111111",
                    WhatsAppNumber = "+201711111111",
                    Website = "https://almokhtaber.com",
                    LaboratoryStatus = Status.Active,
                    OffersHomeSampleCollection = true,
                    HomeSampleCollectionFee = 50m,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-30),
                    VerifierId = verifier.Id,
                    AddressId = labAddresses[0].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                },
                new Laboratory
                {
                    Id = Guid.NewGuid(),
                    Name = "معامل البرج",
                    Description = "أكبر شبكة معامل في مصر. نوفر خدمات تحليل طبي متقدمة مع نتائج دقيقة وسريعة.",
                    Email = "info@alborg.com",
                    UserName = "info@alborg.com",
                    PhoneNumber = "+201722222222",
                    WhatsAppNumber = "+201722222222",
                    Website = "https://alborg.com",
                    LaboratoryStatus = Status.Active,
                    OffersHomeSampleCollection = true,
                    HomeSampleCollectionFee = 60m,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-45),
                    VerifierId = verifier.Id,
                    AddressId = labAddresses[1].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-100)
                },
                new Laboratory
                {
                    Id = Guid.NewGuid(),
                    Name = "معامل ألفا",
                    Description = "معمل تحاليل متخصص في التحاليل الطبية الشاملة والفحوصات الجينية.",
                    Email = "info@alpha-lab.com",
                    UserName = "info@alpha-lab.com",
                    PhoneNumber = "+201733333333",
                    WhatsAppNumber = "+201733333333",
                    Website = "https://alpha-lab.com",
                    LaboratoryStatus = Status.Active,
                    OffersHomeSampleCollection = true,
                    HomeSampleCollectionFee = 45m,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-25),
                    VerifierId = verifier.Id,
                    AddressId = labAddresses[2].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-80)
                },
                new Laboratory
                {
                    Id = Guid.NewGuid(),
                    Name = "معمل المعمل المركزي",
                    Description = "معمل حديث يقدم خدمات التحاليل الطبية بأعلى جودة ودقة في الوقت المحدد.",
                    Email = "info@centrallab.com",
                    UserName = "info@centrallab.com",
                    PhoneNumber = "+201744444444",
                    WhatsAppNumber = "+201744444444",
                    Website = "https://centrallab.com",
                    LaboratoryStatus = Status.Active,
                    OffersHomeSampleCollection = false,
                    HomeSampleCollectionFee = null,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-35),
                    VerifierId = verifier.Id,
                    AddressId = labAddresses[3].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-85)
                },
                new Laboratory
                {
                    Id = Guid.NewGuid(),
                    Name = "معامل كايرو لاب",
                    Description = "معمل تحاليل طبية شامل يستخدم أحدث التقنيات في مجال التحاليل الطبية.",
                    Email = "info@cairolab.com",
                    UserName = "info@cairolab.com",
                    PhoneNumber = "+201755555555",
                    WhatsAppNumber = "+201755555555",
                    Website = "https://cairolab.com",
                    LaboratoryStatus = Status.Active,
                    OffersHomeSampleCollection = true,
                    HomeSampleCollectionFee = 55m,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-20),
                    VerifierId = verifier.Id,
                    AddressId = labAddresses[4].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-70)
                }
            };

            await context.Laboratories.AddRangeAsync(laboratories);
            await context.SaveChangesAsync();

            // Detach and reload laboratories to ensure we have the correct database IDs
            foreach (var lab in laboratories)
            {
                context.Entry(lab).State = EntityState.Detached;
            }
            var savedLaboratoryIds = await context.Laboratories
                .OrderByDescending(l => l.CreatedAt)
                .Take(laboratories.Count)
                .Select(l => l.Id)
                .ToListAsync();

            // Add Working Hours for all saved laboratories
            var workingHours = new List<LabWorkingHours>();
            foreach (var labId in savedLaboratoryIds)
            {
                for (int day = 0; day <= 6; day++)
                {
                    workingHours.Add(new LabWorkingHours
                    {
                        Id = Guid.NewGuid(),
                        LaboratoryId = labId,
                        Day = (DayOfWeek)day,
                        StartTime = new TimeOnly(8, 0),
                        EndTime = day == 5 ? new TimeOnly(14, 0) : new TimeOnly(22, 0), // Friday shorter hours
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.LabWorkingHours.AddRangeAsync(workingHours);
            await context.SaveChangesAsync();

            Console.WriteLine($"{laboratories.Count} Laboratories and {workingHours.Count} Working Hours seeded successfully!");
        }
    }
}