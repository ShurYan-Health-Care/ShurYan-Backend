using Shuryan.Core.Entities.External.Laboratories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILabWorkingHoursRepository : IGenericRepository<LabWorkingHours>
    {
        Task<IEnumerable<LabWorkingHours>> GetWorkingHoursByLaboratoryAsync(Guid laboratoryId);
        Task<bool> IsLaboratoryOpenAsync(Guid laboratoryId, DayOfWeek dayOfWeek, TimeOnly time);
    }
}

