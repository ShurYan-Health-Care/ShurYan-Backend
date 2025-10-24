using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Interfaces.Repositories.Pharmacies
{
    public interface IPharmacyWorkingHoursRepository : IGenericRepository<PharmacyWorkingHours>
    {
        Task<IEnumerable<PharmacyWorkingHours>> GetByPharmacyIdAsync(Guid pharmacyId);
        Task<IEnumerable<PharmacyWorkingHours>> GetByDayOfWeekAsync(SysDayOfWeek dayOfWeek);
        Task<PharmacyWorkingHours?> GetByPharmacyAndDayAsync(Guid pharmacyId, SysDayOfWeek dayOfWeek);
        Task DeleteAllByPharmacyIdAsync(Guid pharmacyId);
        Task<bool> IsPharmacyOpenAsync(Guid pharmacyId, DateTime dateTime);
    }
}
