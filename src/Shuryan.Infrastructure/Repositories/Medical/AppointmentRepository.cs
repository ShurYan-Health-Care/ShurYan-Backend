using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories.Medical
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

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAndDateRangeAsync(
            Guid doctorId, 
            DateTime startDate, 
            DateTime endDate, 
            List<AppointmentStatus>? statuses = null)
        {
            var query = _dbSet
                .AsNoTracking() // نتأكد إن الـ query بيجيب أحدث بيانات من الـ database
                .Where(a => a.DoctorId == doctorId
                    && a.ScheduledStartTime >= startDate
                    && a.ScheduledStartTime < endDate);

            // لو فيه statuses محددة، نفلتر بيها
            if (statuses != null && statuses.Any())
            {
                query = query.Where(a => statuses.Contains(a.Status));
            }

            return await query
                .OrderBy(a => a.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(Guid userId, bool isDoctor)
        {
            var now = DateTime.UtcNow;
            IQueryable<Appointment> query = _dbSet;

            if (isDoctor)
                query = query.Include(a => a.Patient).Where(a => a.DoctorId == userId);
            else
                query = query.Include(a => a.Doctor).Where(a => a.PatientId == userId);

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
                query = query.Include(a => a.Patient)
                             .Include(a => a.DoctorReview)
                             .Include(a => a.Prescription)
                             .Where(a => a.DoctorId == userId);
            else
                query = query.Include(a => a.Doctor)
                             .Include(a => a.DoctorReview)
                             .Include(a => a.Prescription)
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
            var query = _dbSet
                .AsNoTracking() // نتأكد إن الـ query بيجيب أحدث بيانات من الـ database
                .Where(a =>
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

        // Dashboard Statistics Methods
        public async Task<int> GetUniquePatientsCountAsync(Guid doctorId)
        {
            return await _dbSet
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetTodayAppointmentsCountAsync(Guid doctorId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _dbSet
                .Where(a => a.DoctorId == doctorId
                    && a.ScheduledStartTime >= today
                    && a.ScheduledStartTime < tomorrow)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(Guid doctorId)
        {
            return await _dbSet
                .Where(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Completed)
                .SumAsync(a => a.ConsultationFee);
        }

        public async Task<decimal> GetMonthlyRevenueAsync(Guid doctorId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await _dbSet
                .Where(a => a.DoctorId == doctorId
                    && a.Status == AppointmentStatus.Completed
                    && a.ScheduledStartTime >= startDate
                    && a.ScheduledStartTime < endDate)
                .SumAsync(a => a.ConsultationFee);
        }

        public async Task<int> GetPendingAppointmentsCountAsync(Guid doctorId)
        {
            return await _dbSet
                .Where(a => a.DoctorId == doctorId
                    && (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.CheckedIn))
                .CountAsync();
        }

        public async Task<int> GetCancelledAppointmentsCountAsync(Guid doctorId)
        {
            return await _dbSet
                .Where(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Cancelled)
                .CountAsync();
        }

        public async Task<Appointment?> GetDoctorActiveAppointmentAsync(Guid doctorId, Guid? excludeAppointmentId = null)
        {
            var query = _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.InProgress);

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAppointmentId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<(IEnumerable<Appointment> Appointments, int TotalCount)> GetByDoctorIdWithFiltersAsync(
            Guid doctorId,
            DateTime? startDate,
            DateTime? endDate,
            AppointmentStatus? status,
            int pageNumber,
            int pageSize,
            string sortBy,
            string sortOrder)
        {
            // Build base query with necessary includes
            var query = _dbSet
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId);

            // Apply date range filter
            if (startDate.HasValue)
            {
                var startOfDay = startDate.Value.Date;
                query = query.Where(a => a.ScheduledStartTime >= startOfDay);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1);
                query = query.Where(a => a.ScheduledStartTime < endOfDay);
            }

            // Apply status filter
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply custom priority sorting:
            // 1. InProgress (3) first
            // 2. CheckedIn (2) second
            // 3. Rest sorted by ScheduledStartTime
            query = query
                .OrderByDescending(a => a.Status == AppointmentStatus.InProgress ? 3 : 0)
                .ThenByDescending(a => a.Status == AppointmentStatus.CheckedIn ? 2 : 0)
                .ThenByDescending(a => a.Status == AppointmentStatus.Confirmed ? 1 : 0)
                .ThenBy(a => a.ScheduledStartTime);

            // Apply pagination
            var appointments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (appointments, totalCount);
        }

        public async Task<IEnumerable<Appointment>> GetBookedAppointmentsForDateAsync(Guid doctorId, DateTime startOfDay, DateTime endOfDay)
        {
            return await _dbSet
                .Include(a => a.Patient)  // عشان الـ PatientName
                .Where(a => a.DoctorId == doctorId &&
                           a.ScheduledStartTime >= startOfDay &&
                           a.ScheduledStartTime < startOfDay.AddDays(1) &&  // أفضل من endOfDay
                           a.Status != AppointmentStatus.Cancelled &&
                           a.Status != AppointmentStatus.NoShow)
                .OrderBy(a => a.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<Dictionary<AppointmentStatus, int>> GetAppointmentStatisticsByDoctorIdAsync(
            Guid doctorId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(a => a.DoctorId == doctorId);

            // Apply date range filter if provided
            if (startDate.HasValue)
            {
                var startOfDay = startDate.Value.Date;
                query = query.Where(a => a.ScheduledStartTime >= startOfDay);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1);
                query = query.Where(a => a.ScheduledStartTime < endOfDay);
            }

            // Group by status and count
            var statistics = await query
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            return statistics;
        }
    }
}

