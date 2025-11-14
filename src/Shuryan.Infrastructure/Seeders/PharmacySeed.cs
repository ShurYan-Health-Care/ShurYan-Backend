using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class PharmacySeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Pharmacies.AnyAsync())
            {
                Console.WriteLine("Pharmacies already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Pharmacies...");

            var verifier = await context.Verifiers.FirstOrDefaultAsync();
            if (verifier == null)
            {
                Console.WriteLine(" No verifier found. Please seed verifiers first.");
                return;
            }

            // Create Addresses for Pharmacies
            var pharmacyAddresses = new List<Address>
            {
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الجمهورية",
                    City = "الزقازيق",
                    Governorate = Governorate.Sharqia,
                    BuildingNumber = "22",
                    Latitude = 30.5854,
                    Longitude = 31.5039,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع النيل",
                    City = "القاهرة",
                    Governorate = Governorate.Cairo,
                    BuildingNumber = "95",
                    Latitude = 30.0444,
                    Longitude = 31.2357,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الهرم",
                    City = "الجيزة",
                    Governorate = Governorate.Giza,
                    BuildingNumber = "145",
                    Latitude = 30.0131,
                    Longitude = 31.2089,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع سعد زغلول",
                    City = "الإسكندرية",
                    Governorate = Governorate.Alexandria,
                    BuildingNumber = "78",
                    Latitude = 31.2001,
                    Longitude = 29.9187,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الجلاء",
                    City = "المنصورة",
                    Governorate = Governorate.Dakahlia,
                    BuildingNumber = "33",
                    Latitude = 31.0409,
                    Longitude = 31.3785,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع المحطة",
                    City = "طنطا",
                    Governorate = Governorate.Gharbia,
                    BuildingNumber = "56",
                    Latitude = 30.7865,
                    Longitude = 31.0004,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الجامعة",
                    City = "أسيوط",
                    Governorate = Governorate.Assiut,
                    BuildingNumber = "67",
                    Latitude = 27.1809,
                    Longitude = 31.1837,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع التحرير",
                    City = "بورسعيد",
                    Governorate = Governorate.PortSaid,
                    BuildingNumber = "89",
                    Latitude = 31.2653,
                    Longitude = 32.3019,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Addresses.AddRangeAsync(pharmacyAddresses);
            await context.SaveChangesAsync();

            var pharmacies = new List<Pharmacy>
            {
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية العزبي",
                    Description = "صيدلية شاملة توفر جميع أنواع الأدوية والمستحضرات الطبية مع خدمة توصيل مجانية.",
                    Email = "info@alezaby.com",
                    UserName = "info@alezaby.com",
                    PhoneNumber = "+201511111111",
                    WhatsAppNumber = "+201511111111",
                    Website = "https://alezaby-pharmacy.com",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-40),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[0].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدليات سيف",
                    Description = "سلسلة صيدليات موثوقة تقدم أفضل الأدوية والمنتجات الصحية بأسعار تنافسية.",
                    Email = "info@seif-pharmacy.com",
                    UserName = "info@seif-pharmacy.com",
                    PhoneNumber = "+201522222222",
                    WhatsAppNumber = "+201522222222",
                    Website = "https://seif-pharmacy.com",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-35),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[1].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-85)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية الشفاء",
                    Description = "صيدلية حديثة توفر أدوية أصلية ومستحضرات تجميل ومنتجات صحية متنوعة.",
                    Email = "info@elshefaa-pharmacy.com",
                    UserName = "info@elshefaa-pharmacy.com",
                    PhoneNumber = "+201533333333",
                    WhatsAppNumber = "+201533333333",
                    Website = "https://elshefaa-pharmacy.com",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-28),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[2].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-80)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية النهدي",
                    Description = "صيدلية كبرى تقدم خدمات صيدلانية متكاملة مع استشارات مجانية من صيادلة محترفين.",
                    Email = "info@nahdi-pharmacy.com",
                    UserName = "info@nahdi-pharmacy.com",
                    PhoneNumber = "+201544444444",
                    WhatsAppNumber = "+201544444444",
                    Website = "https://nahdi-pharmacy.com",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = false,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-50),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[3].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-100)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية العافية",
                    Description = "صيدلية متخصصة في الأدوية المستوردة والمستحضرات الطبية عالية الجودة.",
                    Email = "info@alafia-pharmacy.com",
                    UserName = "info@alafia-pharmacy.com",
                    PhoneNumber = "+201555555555",
                    WhatsAppNumber = "+201555555555",
                    Website = "https://alafia-pharmacy.com",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-22),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[4].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-75)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية المدينة",
                    Description = "صيدلية عائلية تخدم المجتمع منذ 20 عاماً بكل احترافية وأمانة.",
                    Email = "info@almadina-pharmacy.com",
                    UserName = "info@almadina-pharmacy.com",
                    PhoneNumber = "+201566666666",
                    WhatsAppNumber = "+201566666666",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-32),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[5].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-95)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية الحياة",
                    Description = "صيدلية حديثة توفر جميع احتياجاتك الطبية مع خدمة عملاء ممتازة على مدار اليوم.",
                    Email = "info@alhayat-pharmacy.com",
                    UserName = "info@alhayat-pharmacy.com",
                    PhoneNumber = "+201577777777",
                    WhatsAppNumber = "+201577777777",
                    Website = "https://alhayat-pharmacy.com",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-18),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[6].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-65)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية الدواء",
                    Description = "صيدلية متميزة تقدم أدوية بأسعار مخفضة مع عروض شهرية على المستحضرات الطبية.",
                    Email = "info@eldawaa-pharmacy.com",
                    UserName = "info@eldawaa-pharmacy.com",
                    PhoneNumber = "+201588888888",
                    WhatsAppNumber = "+201588888888",
                    Website = "https://eldawaa-pharmacy.com",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-25),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[7].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-70)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية المستقبل",
                    Description = "صيدلية رقمية حديثة مع نظام إلكتروني متطور لطلب الأدوية وتوصيلها سريعاً.",
                    Email = "info@future-pharmacy.com",
                    UserName = "info@future-pharmacy.com",
                    PhoneNumber = "+201599999999",
                    WhatsAppNumber = "+201599999999",
                    Website = "https://future-pharmacy.com",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.UnderReview,
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[0].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = "صيدلية الأمل",
                    Description = "صيدلية موثوقة توفر أدوية أصلية بأسعار مناسبة مع خدمة استشارات صيدلانية مجانية.",
                    Email = "info@alamal-pharmacy.com",
                    UserName = "info@alamal-pharmacy.com",
                    PhoneNumber = "+201500000000",
                    WhatsAppNumber = "+201500000000",
                    PharmacyStatus = Status.Active,
                    OffersDelivery = true,
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-45),
                    VerifierId = verifier.Id,
                    AddressId = pharmacyAddresses[1].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-105)
                }
            };

            await context.Pharmacies.AddRangeAsync(pharmacies);
            await context.SaveChangesAsync();

            // Detach and reload pharmacies to ensure we have the correct database IDs
            foreach (var pharmacy in pharmacies)
            {
                context.Entry(pharmacy).State = EntityState.Detached;
            }
            var savedPharmacyIds = await context.Pharmacies
                .OrderByDescending(p => p.CreatedAt)
                .Take(pharmacies.Count)
                .Select(p => p.Id)
                .ToListAsync();

            // Add Working Hours for all saved pharmacies
            var workingHours = new List<PharmacyWorkingHours>();
            foreach (var pharmacyId in savedPharmacyIds)
            {
                // Saturday to Thursday
                for (int day = 1; day <= 5; day++)
                {
                    workingHours.Add(new PharmacyWorkingHours
                    {
                        Id = Guid.NewGuid(),
                        PharmacyId = pharmacyId,
                        DayOfWeek = (SysDayOfWeek)day,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(23, 0),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Friday - shorter hours
                workingHours.Add(new PharmacyWorkingHours
                {
                    Id = Guid.NewGuid(),
                    PharmacyId = pharmacyId,
                    DayOfWeek = SysDayOfWeek.Friday,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(15, 0),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.PharmacyWorkingHours.AddRangeAsync(workingHours);
            await context.SaveChangesAsync();

            Console.WriteLine($"{pharmacies.Count} Pharmacies and {workingHours.Count} Working Hours seeded successfully!");
        }
    }
}
