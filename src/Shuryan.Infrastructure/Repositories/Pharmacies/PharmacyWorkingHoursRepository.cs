using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Infrastructure.Data;
using Shuryan.Infrastructure.Repositories;

namespace Shuryan.Infrastructure.Repositories.Pharmacies
{
    public class PharmacyWorkingHoursRepository : GenericRepository<PharmacyWorkingHours>, IPharmacyWorkingHoursRepository
    {
        public PharmacyWorkingHoursRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<PharmacyWorkingHours>> GetByPharmacyIdAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Where(wh => wh.PharmacyId == pharmacyId && !wh.IsDeleted)
                .OrderBy(wh => wh.DayOfWeek)
                .ToListAsync();
        }

        public async Task<IEnumerable<PharmacyWorkingHours>> GetByDayOfWeekAsync(SysDayOfWeek dayOfWeek)
        {
            return await _dbSet
                .Include(wh => wh.Pharmacy)
                .Where(wh => wh.DayOfWeek == dayOfWeek)
                .ToListAsync();
        }

        public async Task<PharmacyWorkingHours?> GetByPharmacyAndDayAsync(Guid pharmacyId, SysDayOfWeek dayOfWeek)
        {
            return await _dbSet
                .FirstOrDefaultAsync(wh => wh.PharmacyId == pharmacyId 
                    && wh.DayOfWeek == dayOfWeek 
                    && !wh.IsDeleted);
        }

        public async Task DeleteAllByPharmacyIdAsync(Guid pharmacyId)
        {
            var workingHours = await _dbSet
                .Where(wh => wh.PharmacyId == pharmacyId)
                .ToListAsync();

            foreach (var wh in workingHours)
            {
                wh.IsDeleted = true;
                wh.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsPharmacyOpenAsync(Guid pharmacyId, DateTime dateTime)
        {
            var dayOfWeek = (SysDayOfWeek)((int)dateTime.DayOfWeek);
            var currentTime = TimeOnly.FromDateTime(dateTime);

            var workingHours = await _dbSet
                .FirstOrDefaultAsync(wh => wh.PharmacyId == pharmacyId 
                    && wh.DayOfWeek == dayOfWeek 
                    && !wh.IsDeleted);

            if (workingHours == null)
                return false;

            return currentTime >= workingHours.StartTime && currentTime <= workingHours.EndTime;
        }
    }
}
