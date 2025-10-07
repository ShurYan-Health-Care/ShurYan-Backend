using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.Pharmacies
{
    /// <summary>
    /// واجهة مخصصة لعمليات قاعدة البيانات الخاصة بطلبات الصيدلية.
    /// </summary>
    public interface IPharmacyOrderRepository : IGenericRepository<PharmacyOrder>
    {
        /// <summary>
        /// يجلب طلب واحد بكل تفاصيله، بما في ذلك بيانات المريض والصيدلية والروشتة المرتبطة به.
        /// </summary>
        /// <param name="orderId">الرقم التعريفي للطلب</param>
        /// <returns>كائن الطلب مع كل تفاصيله أو null إذا لم يتم العثور عليه.</returns>
        Task<PharmacyOrder?> GetOrderWithDetailsAsync(Guid orderId);

        /// <summary>
        /// يجلب كل الطلبات الخاصة بمريض معين مع دعم للـ Pagination.
        /// (مهمة لعرض "تاريخ الطلبات" في حساب المريض)
        /// </summary>
        /// <param name="patientId">الرقم التعريفي للمريض</param>
        /// <param name="pageNumber">رقم الصفحة</param>
        /// <param name="pageSize">حجم الصفحة</param>
        /// <returns>قائمة بطلبات المريض.</returns>
        Task<IEnumerable<PharmacyOrder>> GetPagedOrdersForPatientAsync(Guid patientId, int pageNumber, int pageSize);

        /// <summary>
        /// يجلب كل الطلبات الخاصة بصيدلية معينة مع دعم للـ Pagination وإمكانية الفلترة بالحالة.
        /// (مهمة للوحة تحكم الصيدلية لإدارة الطلبات)
        /// </summary>
        /// <param name="pharmacyId">الرقم التعريفي للصيدلية</param>
        /// <param name="status">فلترة الطلبات حسب حالتها (اختياري)</param>
        /// <param name="pageNumber">رقم الصفحة</param>
        /// <param name="pageSize">حجم الصفحة</param>
        /// <returns>قائمة بطلبات الصيدلية.</returns>
        Task<IEnumerable<PharmacyOrder>> GetPagedOrdersForPharmacyAsync(Guid pharmacyId, PharmacyOrderStatus? status, int pageNumber, int pageSize);

        /// <summary>
        /// يبحث عن طلب باستخدام رقمه الفريد (المعروض للمستخدم).
        /// (مهمة لخدمة العملاء أو للبحث السريع)
        /// </summary>
        /// <param name="orderNumber">رقم الطلب</param>
        /// <returns>كائن الطلب أو null إذا لم يتم العثور عليه.</returns>
        Task<PharmacyOrder?> FindByOrderNumberAsync(string orderNumber);

        /// <summary>
        /// يجلب كل الطلبات التي لها حالة معينة.
        /// (مهمة للتقارير والإحصائيات، مثلاً: معرفة عدد الطلبات التي "خرجت للتوصيل")
        /// </summary>
        /// <param name="status">الحالة المطلوبة</param>
        /// <returns>قائمة بالطلبات التي تطابق الحالة.</returns>
        Task<IEnumerable<PharmacyOrder>> GetOrdersByStatusAsync(PharmacyOrderStatus status);
    }
}
