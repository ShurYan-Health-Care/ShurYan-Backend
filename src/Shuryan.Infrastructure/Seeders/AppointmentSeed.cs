using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Seeders
{
    public static class AppointmentSeed
    {
        public static async Task SeedAsync(ShuryanDbContext context)
        {
            if (await context.Appointments.AnyAsync())
            {
                Console.WriteLine("Appointments already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding Appointments...");

            var patients = await context.Patients.ToListAsync();
            var doctors = await context.Doctors.Include(d => d.Consultations).ToListAsync();
            var appointments = new List<Appointment>();
            var consultationRecords = new List<ConsultationRecord>();

            var random = new Random();

            // Create past completed appointments
            for (int i = 0; i < 20; i++)
            {
                var patient = patients[random.Next(patients.Count)];
                var doctor = doctors[random.Next(doctors.Count)];
                var consultation = doctor.Consultations.FirstOrDefault();

                if (consultation == null) continue;

                var daysAgo = random.Next(7, 90);
                var startTime = DateTime.UtcNow.AddDays(-daysAgo).AddHours(random.Next(9, 15));
                var endTime = startTime.AddMinutes(consultation.SessionDurationMinutes);

                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient.Id,
                    DoctorId = doctor.Id,
                    ScheduledStartTime = startTime,
                    ScheduledEndTime = endTime,
                    ConsultationType = ConsultationTypeEnum.Regular,
                    ConsultationFee = consultation.ConsultationFee,
                    SessionDurationMinutes = consultation.SessionDurationMinutes,
                    Status = AppointmentStatus.Completed,
                    CreatedAt = startTime.AddDays(-2)
                };
                appointments.Add(appointment);

                // Add consultation record
                consultationRecords.Add(new ConsultationRecord
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    ChiefComplaint = "صداع وارتفاع في درجة الحرارة",
                    HistoryOfPresentIllness = "بدأت الأعراض منذ يومين مع فقدان للشهية",
                    PhysicalExamination = "درجة الحرارة 38.5، احتقان في الحلق",
                    Diagnosis = "التهاب في الجهاز التنفسي العلوي",
                    ManagementPlan = "راحة تامة، سوائل دافئة، مضاد حيوي لمدة 5 أيام",
                    CreatedAt = startTime.AddHours(1)
                });
            }

            // Create upcoming appointments
            for (int i = 0; i < 15; i++)
            {
                var patient = patients[random.Next(patients.Count)];
                var doctor = doctors[random.Next(doctors.Count)];
                var consultation = doctor.Consultations.FirstOrDefault();

                if (consultation == null) continue;

                var daysAhead = random.Next(1, 30);
                var startTime = DateTime.UtcNow.AddDays(daysAhead).AddHours(random.Next(9, 15));
                var endTime = startTime.AddMinutes(consultation.SessionDurationMinutes);

                appointments.Add(new Appointment
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient.Id,
                    DoctorId = doctor.Id,
                    ScheduledStartTime = startTime,
                    ScheduledEndTime = endTime,
                    ConsultationType = ConsultationTypeEnum.Regular,
                    ConsultationFee = consultation.ConsultationFee,
                    SessionDurationMinutes = consultation.SessionDurationMinutes,
                    Status = AppointmentStatus.Confirmed,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 5))
                });
            }

            await context.Appointments.AddRangeAsync(appointments);
            await context.ConsultationRecords.AddRangeAsync(consultationRecords);
            await context.SaveChangesAsync();

            Console.WriteLine($"{appointments.Count} Appointments and {consultationRecords.Count} Consultation Records seeded!");
        }
    }
}