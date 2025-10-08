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
        /// يجلب عيادة واحدة مع كل بياناتها المرتبطة بها (الدكتور، العنوان، الصور، أرقام التليفون، الخدمات).
        Task<Clinic?> GetClinicWithDetailsAsync(Guid clinicId);

        /// يجلب العيادة الخاصة بدكتور معين مع كل تفاصيلها.
        /// (مهمة لصفحة ملف الدكتور الشخصي)
        Task<Clinic?> GetClinicByDoctorIdAsync(Guid doctorId);

        /// يبحث عن العيادات بناءً على معايير مختلفة مع دعم للـ Pagination.
        Task<IEnumerable<Clinic>> SearchClinicsAsync(
            string? searchTerm,
            Governorate? governorate,
            MedicalSpecialty? specialty,
            Status? status,
            int pageNumber,
            int pageSize);

        /// يجلب العيادات التي تقدم خدمة معينة.
        /// (مهمة للبحث عن عيادات حسب الخدمة المطلوبة، مثلاً: سونار، رسم قلب)
        Task<IEnumerable<Clinic>> GetClinicsByServiceTypeAsync(ClinicServiceType serviceType);

        /// يجلب العيادات في منطقة جغرافية محددة (بناءً على Latitude & Longitude).
        /// (مهمة لخاصية "أقرب عيادة ليك")
        Task<IEnumerable<Clinic>> GetClinicsNearLocationAsync(double latitude, double longitude, double radiusInKm);

        /// يجلب كل الصور الخاصة بعيادة معينة.
        Task<IEnumerable<ClinicPhoto>> GetClinicPhotosAsync(Guid clinicId);

        /// يجلب كل أرقام التليفون الخاصة بعيادة معينة.
        Task<IEnumerable<ClinicPhoneNumber>> GetClinicPhoneNumbersAsync(Guid clinicId);

        /// يجلب كل الخدمات التي تقدمها عيادة معينة.
        Task<IEnumerable<ClinicService>> GetClinicServicesAsync(Guid clinicId);

        /// يتحقق من وجود عيادة نشطة لدكتور معين.
        /// (مهمة للتحقق قبل إنشاء عيادة جديدة)
        Task<bool> DoctorHasActiveClinicAsync(Guid doctorId);

        /// يجلب العيادات حسب الحالة (نشطة، غير نشطة، معلقة).
        /// (مهمة للإحصائيات ولوحة تحكم الأدمن)
        Task<IEnumerable<Clinic>> GetClinicsByStatusAsync(Status status);

        /// يحدث حالة العيادة (تفعيل/تعطيل/تعليق).
        Task<bool> UpdateClinicStatusAsync(Guid clinicId, Status newStatus);//بس ممكن نمسحه ع هوا موجود فى IGenericRepository
    }
}