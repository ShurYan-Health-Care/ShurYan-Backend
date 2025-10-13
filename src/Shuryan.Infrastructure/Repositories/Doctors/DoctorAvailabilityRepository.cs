using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Doctors
{
    public class DoctorAvailabilityRepository : GenericRepository<DoctorAvailability>, IDoctorAvailabilityRepository
    {
        public DoctorAvailabilityRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(da => da.Doctor)
                .Where(da => da.DoctorId == doctorId && !da.IsDeleted)
                .OrderBy(da => da.DayOfWeek)
                .ThenBy(da => da.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAndDayAsync(Guid doctorId, SysDayOfWeek day)
        {
            return await _dbSet
                .Where(da => da.DoctorId == doctorId && da.DayOfWeek == day && !da.IsDeleted)
                .OrderBy(da => da.StartTime)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingAvailabilityAsync(
            Guid doctorId,
            SysDayOfWeek day,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? excludeId = null)
        {
            var query = _dbSet.Where(da =>
                da.DoctorId == doctorId
                && da.DayOfWeek == day
                && !da.IsDeleted
                && ((da.StartTime < endTime && da.EndTime > startTime)));

            if (excludeId.HasValue)
                query = query.Where(da => da.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}

