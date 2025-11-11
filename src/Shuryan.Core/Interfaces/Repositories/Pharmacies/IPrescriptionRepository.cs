using Shuryan.Core.Entities.External.Pharmacies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories
{
    /// <summary>
    /// واجهة مخصصة لعمليات قاعدة البيانات الخاصة بروشتات الأدوية.
    /// </summary>
    public interface IPrescriptionRepository : IGenericRepository<Prescription>
    {
        /// <summary>
        /// يجلب روشتة واحدة بكل تفاصيلها، بما في ذلك بيانات الدكتور والمريض وقائمة الأدوية الموصوفة.
        /// </summary>
        /// <param name="prescriptionId">الرقم التعريفي للروشتة</param>
        /// <returns>كائن الروشتة مع كل تفاصيلها أو null إذا لم يتم العثور عليها.</returns>
        Task<Prescription?> GetPrescriptionWithDetailsAsync(Guid prescriptionId);

        /// <summary>
        /// يجلب كل الروشتات الخاصة بمريض معين مع دعم للـ Pagination.
        /// (مهمة لعرض التاريخ المرضي والروشتات السابقة للمريض)
        /// </summary>
        /// <param name="patientId">الرقم التعريفي للمريض</param>
        /// <param name="pageNumber">رقم الصفحة</param>
        /// <param name="pageSize">حجم الصفحة</param>
        /// <returns>قائمة بروشتات المريض.</returns>
        Task<IEnumerable<Prescription>> GetPagedPrescriptionsForPatientAsync(Guid patientId, int pageNumber, int pageSize);

        /// <summary>
        /// يجلب كل الروشتات الخاصة بمريض معين مع معلومات الدكتور والحجز
        /// </summary>
        /// <param name="patientId">الرقم التعريفي للمريض</param>
        /// <returns>قائمة بروشتات المريض مع التفاصيل.</returns>
        Task<IEnumerable<Prescription>> GetAllPrescriptionsForPatientWithDetailsAsync(Guid patientId);

        /// <summary>
        /// يجلب الروشتات "النشطة" فقط لمريض معين (التي لم يتم طلبها بعد).
        /// </summary>
        /// <param name="patientId">الرقم التعريفي للمريض</param>
        /// <returns>قائمة بالروشتات النشطة.</returns>
        Task<IEnumerable<Prescription>> GetActivePrescriptionsForPatientAsync(Guid patientId);

        /// <summary>
        /// يبحث عن روشتة باستخدام رقمها الفريد (المكتوب عليها).
        /// (مهمة للاستخدام من قبل الصيدلي للتحقق السريع)
        /// </summary>
        /// <param name="prescriptionNumber">رقم الروشتة</param>
        /// <returns>كائن الروشتة أو null إذا لم يتم العثور عليه.</returns>
        Task<Prescription?> FindByPrescriptionNumberAsync(string prescriptionNumber);

        /// <summary>
        /// يجلب الروشتات التي تم إنشاؤها في فترة زمنية معينة.
        /// (مهمة جدًا للتقارير والإحصائيات في لوحة تحكم الأدمن)
        /// </summary>
        /// <param name="startDate">تاريخ البداية</param>
        /// <param name="endDate">تاريخ النهاية</param>
        /// <returns>قائمة بالروشتات خلال الفترة المحددة.</returns>
        Task<IEnumerable<Prescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// يجلب كل الروشتات التي تحتوي على دواء معين.
        /// (مهمة في حالة سحب دواء من السوق أو لعمل إحصائيات عن الأدوية الأكثر استخدامًا)
        /// </summary>
        /// <param name="medicationId">الرقم التعريفي للدواء</param>
        /// <returns>قائمة بالروشتات التي تحتوي على هذا الدواء.</returns>
        Task<IEnumerable<Prescription>> GetPrescriptionsContainingMedicationAsync(Guid medicationId);
    }
}

