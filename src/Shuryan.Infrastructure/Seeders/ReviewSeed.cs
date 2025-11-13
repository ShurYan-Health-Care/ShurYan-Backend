using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class ReviewSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.DoctorReviews.AnyAsync())
            {
                Console.WriteLine("Reviews already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Reviews...");

            var completedAppointments = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .ToListAsync();

            var reviews = new List<DoctorReview>();
            var random = new Random();

            var comments = new[]
            {
                "دكتور ممتاز ومحترم جداً في التعامل",
                "الكشف كان دقيق والتشخيص صحيح",
                "وقت الانتظار كان طويل لكن الخدمة ممتازة",
                "دكتور متمكن وشرحه واضح",
                "العيادة نظيفة والطاقم متعاون",
                "تجربة جيدة بشكل عام",
                "أنصح بزيارة الدكتور",
                "خدمة احترافية وسعر مناسب"
            };

            foreach (var appointment in completedAppointments.Take(15))
            {
                reviews.Add(new DoctorReview
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    OverallSatisfaction = random.Next(3, 6),
                    WaitingTime = random.Next(3, 6),
                    CommunicationQuality = random.Next(4, 6),
                    ClinicCleanliness = random.Next(3, 6),
                    ValueForMoney = random.Next(3, 6),
                    Comment = comments[random.Next(comments.Length)],
                    IsAnonymous = random.Next(2) == 0,
                    CreatedAt = appointment.ScheduledStartTime.AddDays(random.Next(1, 3))
                });
            }

            await context.DoctorReviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();

            Console.WriteLine($"{reviews.Count} Doctor Reviews seeded successfully!");
        }
    }
}