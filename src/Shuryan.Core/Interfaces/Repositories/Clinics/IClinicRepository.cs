using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Clinic;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.Clinics
{
    /// <summary>
    /// واجهة مخصصة لعمليات قاعدة البيانات الخاصة بالعيادات.
    /// </summary>
    public interface IClinicRepository : IGenericRepository<Clinic>
    {
        /// <summary>
        /// يجلب عيادة واحدة مع كل بياناتها المرتبطة بها (الدكتور، العنوان، الصور، أرقام التليفون، الخدمات).
        /// </summary>
        /// <param name="clinicId">الرقم التعريفي للعيادة</param>
        /// <returns>كائن العيادة مع كل تفاصيله أو null إذا لم يتم العثور عليه.</returns>
        Task<Clinic?> GetClinicWithDetailsAsync(Guid clinicId);

        /// <summary>
        /// يجلب العيادة الخاصة بدكتور معين مع كل تفاصيلها.
        /// (مهمة لصفحة ملف الدكتور الشخصي)
        /// </summary>
        /// <param name="doctorId">الرقم التعريفي للدكتور</param>
        /// <returns>كائن العيادة أو null إذا لم يكن للدكتور عيادة.</returns>
        Task<Clinic?> GetClinicByDoctorIdAsync(Guid doctorId);

        /// <summary>
        /// يبحث عن العيادات بناءً على معايير مختلفة مع دعم للـ Pagination.
        /// </summary>
        /// <param name="searchTerm">نص البحث (جزء من اسم العيادة أو اسم الدكتور)</param>
        /// <param name="governorate">فلترة حسب المحافظة</param>
        /// <param name="specialty">فلترة حسب تخصص الدكتور</param>
        /// <param name="status">فلترة حسب حالة العيادة</param>
        /// <param name="pageNumber">رقم الصفحة الحالية</param>
        /// <param name="pageSize">عدد العيادات في كل صفحة</param>
        /// <returns>قائمة بالعيادات التي تطابق معايير البحث.</returns>
        Task<IEnumerable<Clinic>> SearchClinicsAsync(
            string? searchTerm,
            Governorate? governorate,
            MedicalSpecialty? specialty,
            Status? status,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// يجلب العيادات التي تقدم خدمة معينة.
        /// (مهمة للبحث عن عيادات حسب الخدمة المطلوبة، مثلاً: سونار، رسم قلب)
        /// </summary>
        /// <param name="serviceType">نوع الخدمة المطلوبة</param>
        /// <returns>قائمة بالعيادات التي تقدم هذه الخدمة.</returns>
        Task<IEnumerable<Clinic>> GetClinicsByServiceTypeAsync(ClinicServiceType serviceType);

        /// <summary>
        /// يجلب العيادات في منطقة جغرافية محددة (بناءً على Latitude & Longitude).
        /// (مهمة لخاصية "أقرب عيادة ليك")
        /// </summary>
        /// <param name="latitude">خط العرض</param>
        /// <param name="longitude">خط الطول</param>
        /// <param name="radiusInKm">نطاق البحث بالكيلومتر</param>
        /// <returns>قائمة بالعيادات القريبة.</returns>
        Task<IEnumerable<Clinic>> GetClinicsNearLocationAsync(double latitude, double longitude, double radiusInKm);

        /// <summary>
        /// يجلب كل الصور الخاصة بعيادة معينة.
        /// </summary>
        /// <param name="clinicId">الرقم التعريفي للعيادة</param>
        /// <returns>قائمة بصور العيادة.</returns>
        Task<IEnumerable<ClinicPhoto>> GetClinicPhotosAsync(Guid clinicId);

        /// <summary>
        /// يجلب كل أرقام التليفون الخاصة بعيادة معينة.
        /// </summary>
        /// <param name="clinicId">الرقم التعريفي للعيادة</param>
        /// <returns>قائمة بأرقام تليفون العيادة.</returns>
        Task<IEnumerable<ClinicPhoneNumber>> GetClinicPhoneNumbersAsync(Guid clinicId);

        /// <summary>
        /// يجلب كل الخدمات التي تقدمها عيادة معينة.
        /// </summary>
        /// <param name="clinicId">الرقم التعريفي للعيادة</param>
        /// <returns>قائمة بخدمات العيادة.</returns>
        Task<IEnumerable<ClinicService>> GetClinicServicesAsync(Guid clinicId);

        /// <summary>
        /// يتحقق من وجود عيادة نشطة لدكتور معين.
        /// (مهمة للتحقق قبل إنشاء عيادة جديدة)
        /// </summary>
        /// <param name="doctorId">الرقم التعريفي للدكتور</param>
        /// <returns>true إذا كان الدكتور لديه عيادة نشطة، false إذا لم يكن.</returns>
        Task<bool> DoctorHasActiveClinicAsync(Guid doctorId);

        /// <summary>
        /// يجلب العيادات حسب الحالة (نشطة، غير نشطة، معلقة).
        /// (مهمة للإحصائيات ولوحة تحكم الأدمن)
        /// </summary>
        /// <param name="status">حالة العيادة</param>
        /// <returns>قائمة بالعيادات التي تطابق الحالة.</returns>
        Task<IEnumerable<Clinic>> GetClinicsByStatusAsync(Status status);

        /// <summary>
        /// يحدث حالة العيادة (تفعيل/تعطيل/تعليق).
        /// </summary>
        /// <param name="clinicId">الرقم التعريفي للعيادة</param>
        /// <param name="newStatus">الحالة الجديدة</param>
        /// <returns>true إذا تم التحديث بنجاح، false إذا لم يتم.</returns>
        Task<bool> UpdateClinicStatusAsync(Guid clinicId, Status newStatus);//بس ممكن نمسحه ع هوا موجود فى IGenericRepository
    }
}