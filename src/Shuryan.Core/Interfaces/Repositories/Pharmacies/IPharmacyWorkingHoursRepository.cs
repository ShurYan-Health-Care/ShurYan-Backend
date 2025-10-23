using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Interfaces.Repositories.Pharmacies
{
    /// <summary>
    /// واجهة مخصصة لعمليات قاعدة البيانات الخاصة بمواعيد عمل الصيدليات.
    /// </summary>
    public interface IPharmacyWorkingHoursRepository : IGenericRepository<PharmacyWorkingHours>
    {
        /// <summary>
        /// يجلب كل مواعيد العمل لصيدلية معينة
        /// </summary>
        Task<IEnumerable<PharmacyWorkingHours>> GetByPharmacyIdAsync(Guid pharmacyId);

        /// <summary>
        /// يجلب مواعيد عمل يوم محدد لصيدلية معينة
        /// </summary>
        Task<PharmacyWorkingHours?> GetByPharmacyAndDayAsync(Guid pharmacyId, SysDayOfWeek dayOfWeek);

        /// <summary>
        /// يحذف كل مواعيد العمل لصيدلية معينة
        /// </summary>
        Task DeleteAllByPharmacyIdAsync(Guid pharmacyId);

        /// <summary>
        /// يتحقق إذا كانت الصيدلية مفتوحة في وقت محدد
        /// </summary>
        Task<bool> IsPharmacyOpenAsync(Guid pharmacyId, DateTime dateTime);
    }
}
