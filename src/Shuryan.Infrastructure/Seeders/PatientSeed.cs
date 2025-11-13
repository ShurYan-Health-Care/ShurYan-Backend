using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class PatientSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Patients.AnyAsync())
            {
                Console.WriteLine("Patients already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Patients...");

            // Create Addresses for Patients
            var patientAddresses = new List<Address>
            {
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع النصر",
                    City = "الزقازيق",
                    Governorate = Governorate.Sharqia,
                    BuildingNumber = "15",
                    Latitude = 30.5877,
                    Longitude = 31.5029,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الجمهورية",
                    City = "القاهرة",
                    Governorate = Governorate.Cairo,
                    BuildingNumber = "42",
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
                    BuildingNumber = "88",
                    Latitude = 30.0131,
                    Longitude = 31.2089,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الكورنيش",
                    City = "الإسكندرية",
                    Governorate = Governorate.Alexandria,
                    BuildingNumber = "23",
                    Latitude = 31.2001,
                    Longitude = 29.9187,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع المحطة",
                    City = "المنصورة",
                    Governorate = Governorate.Dakahlia,
                    BuildingNumber = "67",
                    Latitude = 31.0409,
                    Longitude = 31.3785,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع الجلاء",
                    City = "طنطا",
                    Governorate = Governorate.Gharbia,
                    BuildingNumber = "12",
                    Latitude = 30.7865,
                    Longitude = 31.0004,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع سعد زغلول",
                    City = "أسيوط",
                    Governorate = Governorate.Assiut,
                    BuildingNumber = "34",
                    Latitude = 27.1809,
                    Longitude = 31.1837,
                    CreatedAt = DateTime.UtcNow
                },
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = "شارع البحر",
                    City = "بورسعيد",
                    Governorate = Governorate.PortSaid,
                    BuildingNumber = "56",
                    Latitude = 31.2653,
                    Longitude = 32.3019,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Addresses.AddRangeAsync(patientAddresses);
            await context.SaveChangesAsync();

            var patients = new List<Patient>
            {
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "أحمد",
                    LastName = "محمود",
                    Email = "ahmed.mahmoud@patient.com",
                    UserName = "ahmed.mahmoud@patient.com",
                    PhoneNumber = "+201001111111",
                    BirthDate = new DateTime(1990, 5, 15),
                    Gender = Gender.Male,
                    AddressId = patientAddresses[0].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "فاطمة",
                    LastName = "علي",
                    Email = "fatma.ali@patient.com",
                    UserName = "fatma.ali@patient.com",
                    PhoneNumber = "+201002222222",
                    BirthDate = new DateTime(1985, 8, 22),
                    Gender = Gender.Female,
                    AddressId = patientAddresses[1].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "محمد",
                    LastName = "السيد",
                    Email = "mohamed.elsayed@patient.com",
                    UserName = "mohamed.elsayed@patient.com",
                    PhoneNumber = "+201003333333",
                    BirthDate = new DateTime(1995, 3, 10),
                    Gender = Gender.Male,
                    AddressId = patientAddresses[2].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "مريم",
                    LastName = "حسن",
                    Email = "mariam.hassan@patient.com",
                    UserName = "mariam.hassan@patient.com",
                    PhoneNumber = "+201004444444",
                    BirthDate = new DateTime(1992, 11, 30),
                    Gender = Gender.Female,
                    AddressId = patientAddresses[3].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "خالد",
                    LastName = "إبراهيم",
                    Email = "khaled.ibrahim@patient.com",
                    UserName = "khaled.ibrahim@patient.com",
                    PhoneNumber = "+201005555555",
                    BirthDate = new DateTime(1988, 7, 18),
                    Gender = Gender.Male,
                    AddressId = patientAddresses[4].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "سارة",
                    LastName = "أحمد",
                    Email = "sara.ahmed@patient.com",
                    UserName = "sara.ahmed@patient.com",
                    PhoneNumber = "+201006666666",
                    BirthDate = new DateTime(1997, 2, 5),
                    Gender = Gender.Female,
                    AddressId = patientAddresses[5].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "يوسف",
                    LastName = "عبدالله",
                    Email = "youssef.abdullah@patient.com",
                    UserName = "youssef.abdullah@patient.com",
                    PhoneNumber = "+201007777777",
                    BirthDate = new DateTime(1993, 9, 12),
                    Gender = Gender.Male,
                    AddressId = patientAddresses[6].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "نور",
                    LastName = "محمد",
                    Email = "nour.mohamed@patient.com",
                    UserName = "nour.mohamed@patient.com",
                    PhoneNumber = "+201008888888",
                    BirthDate = new DateTime(1991, 12, 25),
                    Gender = Gender.Female,
                    AddressId = patientAddresses[7].Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "عمر",
                    LastName = "خالد",
                    Email = "omar.khaled@patient.com",
                    UserName = "omar.khaled@patient.com",
                    PhoneNumber = "+201009999999",
                    BirthDate = new DateTime(1989, 4, 8),
                    Gender = Gender.Male,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "ليلى",
                    LastName = "حسين",
                    Email = "laila.hussein@patient.com",
                    UserName = "laila.hussein@patient.com",
                    PhoneNumber = "+201000000000",
                    BirthDate = new DateTime(1994, 6, 20),
                    Gender = Gender.Female,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Patients.AddRangeAsync(patients);
            await context.SaveChangesAsync();

            Console.WriteLine($"{patients.Count} Patients and {patientAddresses.Count} Addresses seeded successfully!");
        }
    }
}
