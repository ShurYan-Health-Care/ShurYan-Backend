using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Appointments;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Appointment?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Clinic)
                .Include(a => a.ConsultationRecord)
                .Include(a => a.Prescription)
                .Include(a => a.LabPrescription)
                .Include(a => a.DoctorReview)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId)
        {
            return await _dbSet
                .Include(a => a.Doctor)
                .Include(a => a.ConsultationRecord)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.ConsultationRecord)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAndDateAsync(Guid doctorId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _dbSet
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId
                    && a.ScheduledStartTime >= startOfDay
                    && a.ScheduledStartTime < endOfDay)
                .OrderBy(a => a.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(Guid userId, bool isDoctor)
        {
            var now = DateTime.UtcNow;
            IQueryable<Appointment> query = _dbSet;

            if (isDoctor)
                query = query.Include(a => a.Doctor).Where(a => a.DoctorId == userId);
            else
                query = query.Include(a => a.Patient).Where(a => a.PatientId == userId);

            return await query
                .Where(a => a.ScheduledStartTime >= now
                    && (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.CheckedIn))
                .OrderBy(a => a.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetPastAppointmentsAsync(Guid userId, bool isDoctor)
        {
            var now = DateTime.UtcNow;
            IQueryable<Appointment> query = _dbSet;

            if (isDoctor)
                query = query.Include(a => a.Doctor)
                             .Include(a => a.DoctorReview)
                             .Where(a => a.DoctorId == userId);
            else
                query = query.Include(a => a.Patient)
                             .Include(a => a.DoctorReview)
                             .Where(a => a.PatientId == userId);

            return await query
                .Where(a => a.ScheduledEndTime < now || a.Status == AppointmentStatus.Completed)
                .OrderByDescending(a => a.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status)
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Status == status)
                .OrderBy(a => a.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null)
        {
            var query = _dbSet.Where(a =>
                a.DoctorId == doctorId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && ((a.ScheduledStartTime < endTime && a.ScheduledEndTime > startTime)));

            if (excludeAppointmentId.HasValue)
                query = query.Where(a => a.Id != excludeAppointmentId.Value);

            return await query.AnyAsync();
        }

        public async Task<int> GetCompletedAppointmentsCountAsync(Guid doctorId)
        {
            return await _dbSet
                .Where(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Completed)
                .CountAsync();
        }
    }
}