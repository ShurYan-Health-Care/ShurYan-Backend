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
        public PharmacyWorkingHoursRepository(ShuryanDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PharmacyWorkingHours>> GetByPharmacyIdAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Where(wh => wh.PharmacyId == pharmacyId)
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
                .FirstOrDefaultAsync(wh => wh.PharmacyId == pharmacyId && wh.DayOfWeek == dayOfWeek);
        }
    }
}
