using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories
{
    public class DoctorAvailabilityRepository : GenericRepository<DoctorAvailability>, IDoctorAvailabilityRepository
    {
        private readonly ShuryanDbContext _context;

        public DoctorAvailabilityRepository(ShuryanDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _context.DoctorAvailability
                .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                .OrderBy(d => d.DayOfWeek)
                .ThenBy(d => d.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAndDayAsync(Guid doctorId, SysDayOfWeek day)
        {
            return await _context.DoctorAvailability
                .Where(d => d.DoctorId == doctorId &&
                           d.DayOfWeek == day &&
                           !d.IsDeleted)
                .OrderBy(d => d.StartTime)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingAvailabilityAsync(
            Guid doctorId,
            SysDayOfWeek day,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? excludeId = null)
        {
            var query = _context.DoctorAvailability
                .Where(d => d.DoctorId == doctorId &&
                           d.DayOfWeek == day &&
                           !d.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            var hasOverlap = await query.AnyAsync(d =>
                startTime < d.EndTime && endTime > d.StartTime);

            return hasOverlap;
        }
    }
}