using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Enums.Clinic;
using Shuryan.Core.Enums;
using Shuryan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Shared;

namespace Shuryan.Infrastructure.Seeders
{
    public static class ClinicSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Clinics.AnyAsync())
            {
                Console.WriteLine("Clinics already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Clinics...");

            var doctors = await context.Doctors.Take(5).ToListAsync();
            var clinics = new List<Clinic>();
            var clinicPhones = new List<ClinicPhoneNumber>();
            var clinicServices = new List<ClinicService>();

            // Create Addresses for Clinics
            var clinicAddresses = new List<Address>
            {
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع المستشفى",
                    City = "الزقازيق",
                    Governorate = Governorate.Sharqia,
                    BuildingNumber = "10",
                    Latitude = 30.5877,
                    Longitude = 31.5029,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الطيران",
                    City = "القاهرة",
                    Governorate = Governorate.Cairo,
                    BuildingNumber = "25",
                    Latitude = 30.0626,
                    Longitude = 31.2497,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الدقي",
                    City = "الجيزة",
                    Governorate = Governorate.Giza,
                    BuildingNumber = "18",
                    Latitude = 30.0444,
                    Longitude = 31.2089,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع المعمورة",
                    City = "الإسكندرية",
                    Governorate = Governorate.Alexandria,
                    BuildingNumber = "40",
                    Latitude = 31.2156,
                    Longitude = 29.9553,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع البنك الأهلي",
                    City = "المنصورة",
                    Governorate = Governorate.Dakahlia,
                    BuildingNumber = "32",
                    Latitude = 31.0409,
                    Longitude = 31.3785,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Addresses.AddRangeAsync(clinicAddresses);
            await context.SaveChangesAsync();

            for (int i = 0; i < doctors.Count; i++)
            {
                var clinic = new Clinic
                {
                    Id = Guid.NewGuid(),
                    Name = $"عيادة د. {doctors[i].FirstName} {doctors[i].LastName}",
                    DoctorId = doctors[i].Id,
                    AddressId = clinicAddresses[i].Id,
                    FacilityVideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    CreatedAt = DateTime.UtcNow
                };
                clinics.Add(clinic);

                // Add phone numbers
                clinicPhones.Add(new ClinicPhoneNumber
                {
                    Id = Guid.NewGuid(),
                    ClinicId = clinic.Id,
                    Number = $"+20155000000{i}",
                    Type = ClinicPhoneNumberType.Mobile,
                    CreatedAt = DateTime.UtcNow
                });

                clinicPhones.Add(new ClinicPhoneNumber
                {
                    Id = Guid.NewGuid(),
                    ClinicId = clinic.Id,
                    Number = $"+2004000000{i}",
                    Type = ClinicPhoneNumberType.Landline,
                    CreatedAt = DateTime.UtcNow
                });

                // Add services
                clinicServices.Add(new ClinicService
                {
                    Id = Guid.NewGuid(),
                    ClinicId = clinic.Id,
                    ServiceType = ClinicServiceType.Examination,
                    CreatedAt = DateTime.UtcNow
                });

                clinicServices.Add(new ClinicService
                {
                    Id = Guid.NewGuid(),
                    ClinicId = clinic.Id,
                    ServiceType = ClinicServiceType.ECG,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.Clinics.AddRangeAsync(clinics);
            await context.ClinicPhoneNumbers.AddRangeAsync(clinicPhones);
            await context.ClinicServices.AddRangeAsync(clinicServices);
            await context.SaveChangesAsync();

            Console.WriteLine($"{clinics.Count} Clinics, {clinicPhones.Count} Phone Numbers, and {clinicServices.Count} Services seeded!");
        }
    }
}