using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Laboratories
{
    public class LabWorkingHoursRepository : GenericRepository<LabWorkingHours>, ILabWorkingHoursRepository
    {
        public LabWorkingHoursRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<LabWorkingHours>> GetWorkingHoursByLaboratoryAsync(Guid laboratoryId)
        {
            return await _dbSet
                .Where(wh => wh.LaboratoryId == laboratoryId)
                .OrderBy(wh => wh.Day)
                .ToListAsync();
        }

        public async Task<bool> IsLaboratoryOpenAsync(Guid laboratoryId, DayOfWeek dayOfWeek, TimeOnly time)
        {
            return await _dbSet
                .AnyAsync(wh => wh.LaboratoryId == laboratoryId 
                    && wh.Day == dayOfWeek 
                    && wh.IsActive 
                    && time >= wh.StartTime 
                    && time <= wh.EndTime);
        }
    }
}

