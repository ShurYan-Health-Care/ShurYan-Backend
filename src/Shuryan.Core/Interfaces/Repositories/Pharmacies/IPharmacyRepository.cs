using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Interfaces.Repositories.Pharmacies
{
    public interface IPharmacyRepository : IGenericRepository<Pharmacy>
    {
        /// <summary>
        /// يجلب صيدلية واحدة مع كل بياناتها المرتبطة بها (العنوان، مواعيد العمل، المستندات).
        /// </summary>
        /// <param name="pharmacyId">الرقم التعريفي للصيدلية</param>
        /// <returns>كائن الصيدلية مع كل تفاصيله أو null إذا لم يتم العثور عليه.</returns>
        Task<Pharmacy?> GetPharmacyWithDetailsAsync(Guid pharmacyId);

        /// <summary>
        /// يبحث عن الصيدليات بناءً على معايير مختلفة مع دعم للـ Pagination.
        /// </summary>
        /// <param name="searchTerm">نص البحث (جزء من اسم الصيدلية)</param>
        /// <param name="governorate">فلترة حسب المحافظة</param>
        /// <param name="offersDelivery">فلترة حسب توفر خدمة التوصيل</param>
        /// <param name="pageNumber">رقم الصفحة الحالية</param>
        /// <param name="pageSize">عدد الصيدليات في كل صفحة</param>
        /// <returns>قائمة بالصيدليات التي تطابق معايير البحث.</returns>
        Task<IEnumerable<Pharmacy>> SearchPharmaciesAsync(
                                        string? searchTerm,
                                        Governorate? governorate,
                                        bool? offersDelivery,
                                        int pageNumber,
                                        int pageSize);

        /// <summary>
        /// يجلب قائمة بالصيدليات التي تنتظر المراجعة والتوثيق.
        /// (مهمة للـ Admin/Verifier Dashboard)
        /// </summary>
        /// <returns>قائمة بالصيدليات التي في حالة "تحت المراجعة".</returns>
        Task<IEnumerable<Pharmacy>> GetPendingVerificationPharmaciesAsync();

        /// <summary>
        /// يبحث عن الصيدليات التي تحتوي على قائمة معينة من الأدوية.
        /// (مهمة لخاصية "البحث الذكي عن صيدلية" بعد إصدار الروشتة)
        /// </summary>
        /// <param name="medicationIds">قائمة بالـ IDs الخاصة بالأدوية المطلوبة</param>
        /// <returns>قائمة بالصيدليات التي توفر كل هذه الأدوية.</returns>
        Task<IEnumerable<Pharmacy>> GetPharmaciesByMedicationsAsync(IEnumerable<Guid> medicationIds);
    }
}
