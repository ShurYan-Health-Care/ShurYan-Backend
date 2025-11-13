using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class DoctorSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Doctors.AnyAsync())
            {
                Console.WriteLine("Doctors already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Doctors...");

            // Get verifiers to assign to doctors
            var verifiers = await context.Verifiers.Take(3).ToListAsync();
            var verifierId = verifiers.Any() ? verifiers[0].Id : (Guid?)null;

            var doctors = new List<Doctor>
            {
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "أحمد",
                    LastName = "العربي",
                    Email = "ahmed.alaraby@doctor.com",
                    UserName = "ahmed.alaraby@doctor.com",
                    PhoneNumber = "+201101111111",
                    BirthDate = new DateTime(1980, 3, 15),
                    Gender = Gender.Male,
                    MedicalSpecialty = MedicalSpecialty.Cardiology,
                    YearsOfExperience = 15,
                    Biography = "استشاري أمراض القلب والأوعية الدموية مع خبرة واسعة في تشخيص وعلاج أمراض القلب المختلفة.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-30),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "سارة",
                    LastName = "محمود",
                    Email = "sara.mahmoud@doctor.com",
                    UserName = "sara.mahmoud@doctor.com",
                    PhoneNumber = "+201102222222",
                    BirthDate = new DateTime(1985, 7, 22),
                    Gender = Gender.Female,
                    MedicalSpecialty = MedicalSpecialty.Pediatrics,
                    YearsOfExperience = 10,
                    Biography = "أخصائية طب الأطفال متخصصة في رعاية حديثي الولادة والأطفال حتى سن المراهقة.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-25),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-55)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "محمد",
                    LastName = "حسن",
                    Email = "mohamed.hassan@doctor.com",
                    UserName = "mohamed.hassan@doctor.com",
                    PhoneNumber = "+201103333333",
                    BirthDate = new DateTime(1978, 11, 10),
                    Gender = Gender.Male,
                    MedicalSpecialty = MedicalSpecialty.Orthopedics,
                    YearsOfExperience = 18,
                    Biography = "استشاري جراحة العظام والمفاصل مع خبرة في جراحات المفاصل الصناعية وإصابات الملاعب.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-45),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-70)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "فاطمة",
                    LastName = "علي",
                    Email = "fatma.ali@doctor.com",
                    UserName = "fatma.ali@doctor.com",
                    PhoneNumber = "+201104444444",
                    BirthDate = new DateTime(1983, 5, 18),
                    Gender = Gender.Female,
                    MedicalSpecialty = MedicalSpecialty.ObstetricsGynecology,
                    YearsOfExperience = 12,
                    Biography = "استشارية أمراض النساء والتوليد متخصصة في متابعة الحمل والولادة الطبيعية والقيصرية.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-20),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-50)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "خالد",
                    LastName = "إبراهيم",
                    Email = "khaled.ibrahim@doctor.com",
                    UserName = "khaled.ibrahim@doctor.com",
                    PhoneNumber = "+201105555555",
                    BirthDate = new DateTime(1982, 9, 5),
                    Gender = Gender.Male,
                    MedicalSpecialty = MedicalSpecialty.Dermatology,
                    YearsOfExperience = 13,
                    Biography = "أخصائي أمراض الجلدية والتجميل مع خبرة في علاج الأمراض الجلدية المختلفة والليزر.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-35),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-65)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "منى",
                    LastName = "السيد",
                    Email = "mona.elsayed@doctor.com",
                    UserName = "mona.elsayed@doctor.com",
                    PhoneNumber = "+201106666666",
                    BirthDate = new DateTime(1987, 2, 28),
                    Gender = Gender.Female,
                    MedicalSpecialty = MedicalSpecialty.Ophthalmology,
                    YearsOfExperience = 8,
                    Biography = "أخصائية طب وجراحة العيون متخصصة في علاج المياه البيضاء والزرقاء وتصحيح النظر.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-15),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-40)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "عمر",
                    LastName = "خالد",
                    Email = "omar.khaled@doctor.com",
                    UserName = "omar.khaled@doctor.com",
                    PhoneNumber = "+201107777777",
                    BirthDate = new DateTime(1981, 12, 12),
                    Gender = Gender.Male,
                    MedicalSpecialty = MedicalSpecialty.GeneralSurgery,
                    YearsOfExperience = 14,
                    Biography = "استشاري جراحة عامة متخصص في جراحات المناظير والجراحات الدقيقة.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-40),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-75)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "نادية",
                    LastName = "أحمد",
                    Email = "nadia.ahmed@doctor.com",
                    UserName = "nadia.ahmed@doctor.com",
                    PhoneNumber = "+201108888888",
                    BirthDate = new DateTime(1986, 6, 20),
                    Gender = Gender.Female,
                    MedicalSpecialty = MedicalSpecialty.Neurology,
                    YearsOfExperience = 9,
                    Biography = "أخصائية طب الأعصاب متخصصة في علاج أمراض الجهاز العصبي والصداع المزمن.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-28),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-58)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "يوسف",
                    LastName = "عبدالله",
                    Email = "youssef.abdullah@doctor.com",
                    UserName = "youssef.abdullah@doctor.com",
                    PhoneNumber = "+201109999999",
                    BirthDate = new DateTime(1984, 4, 8),
                    Gender = Gender.Male,
                    MedicalSpecialty = MedicalSpecialty.ENT,
                    YearsOfExperience = 11,
                    Biography = "أخصائي أنف وأذن وحنجرة مع خبرة في علاج التهابات الأذن واضطرابات السمع.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-22),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-52)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "ياسمين",
                    LastName = "محمد",
                    Email = "yasmin.mohamed@doctor.com",
                    UserName = "yasmin.mohamed@doctor.com",
                    PhoneNumber = "+201100000000",
                    BirthDate = new DateTime(1988, 10, 15),
                    Gender = Gender.Female,
                    MedicalSpecialty = MedicalSpecialty.Psychiatry,
                    YearsOfExperience = 7,
                    Biography = "أخصائية الطب النفسي متخصصة في علاج الاكتئاب والقلق والاضطرابات النفسية.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-18),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "حسام",
                    LastName = "فتحي",
                    Email = "hossam.fathy@doctor.com",
                    UserName = "hossam.fathy@doctor.com",
                    PhoneNumber = "+201111111111",
                    BirthDate = new DateTime(1979, 8, 25),
                    Gender = Gender.Male,
                    MedicalSpecialty = MedicalSpecialty.Gastroenterology,
                    YearsOfExperience = 16,
                    Biography = "استشاري أمراض الجهاز الهضمي والكبد مع خبرة في المناظير التشخيصية والعلاجية.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-50),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-80)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "ريم",
                    LastName = "صالح",
                    Email = "reem.saleh@doctor.com",
                    UserName = "reem.saleh@doctor.com",
                    PhoneNumber = "+201122222222",
                    BirthDate = new DateTime(1990, 1, 30),
                    Gender = Gender.Female,
                    MedicalSpecialty = MedicalSpecialty.GeneralMedicine,
                    YearsOfExperience = 5,
                    Biography = "طبيبة عامة متخصصة في الرعاية الصحية الأولية والكشف الطبي الشامل.",
                    VerificationStatus = VerificationStatus.UnderReview,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "طارق",
                    LastName = "عبدالرحمن",
                    Email = "tarek.abdelrahman@doctor.com",
                    UserName = "tarek.abdelrahman@doctor.com",
                    PhoneNumber = "+201133333333",
                    BirthDate = new DateTime(1983, 3, 5),
                    Gender = Gender.Male,
                    MedicalSpecialty = MedicalSpecialty.Urology,
                    YearsOfExperience = 12,
                    Biography = "استشاري المسالك البولية وأمراض الذكورة مع خبرة في جراحات المناظير.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-32),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-62)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "لينا",
                    LastName = "حسين",
                    Email = "lina.hussein@doctor.com",
                    UserName = "lina.hussein@doctor.com",
                    PhoneNumber = "+201144444444",
                    BirthDate = new DateTime(1989, 11, 18),
                    Gender = Gender.Female,
                    MedicalSpecialty = MedicalSpecialty.Endocrinology,
                    YearsOfExperience = 6,
                    Biography = "أخصائية الغدد الصماء والسكري متخصصة في علاج أمراض الغدة الدرقية والسكري.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-12),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-35)
                },
                new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "وليد",
                    LastName = "جمال",
                    Email = "walid.gamal@doctor.com",
                    UserName = "walid.gamal@doctor.com",
                    PhoneNumber = "+201155555555",
                    BirthDate = new DateTime(1985, 7, 9),
                    Gender = Gender.Male,
                    MedicalSpecialty = MedicalSpecialty.EmergencyMedicine,
                    YearsOfExperience = 10,
                    Biography = "أخصائي طب الطوارئ مع خبرة واسعة في التعامل مع الحالات الحرجة والإنعاش القلبي.",
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedAt = DateTime.UtcNow.AddDays(-26),
                    VerifierId = verifierId,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-56)
                }
            };

            await context.Doctors.AddRangeAsync(doctors);
            await context.SaveChangesAsync();

            Console.WriteLine($"{doctors.Count} Doctors seeded successfully!");
        }
    }
}
