using Microsoft.EntityFrameworkCore.Storage;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.Repositories.ChatRepositories;
using Shuryan.Core.Interfaces.Repositories.ClinicRepositories;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Core.Interfaces.Repositories.MedicationRepositories;
using Shuryan.Core.Interfaces.Repositories.ReviewRepositories;

namespace Shuryan.Core.Interfaces.UnitOfWork
{
	public interface IUnitOfWork : IDisposable
    {
        // ==================== Doctor Related Repositories ====================
        IDoctorRepository Doctors { get; }
        IDoctorAvailabilityRepository DoctorAvailabilities { get; }
        IDoctorConsultationRepository DoctorConsultations { get; }
        IDoctorOverrideRepository DoctorOverrides { get; }
        IDoctorDocumentRepository DoctorDocuments { get; }
        IDoctorPartnerSuggestionRepository DoctorPartnerSuggestions { get; }

        // ==================== Patient Related Repositories ====================
        IPatientRepository Patients { get; }
        IMedicalHistoryItemRepository MedicalHistoryItems { get; }

        // ==================== Medical/Appointment Related Repositories ====================
        IAppointmentRepository Appointments { get; }
        IConsultationRecordRepository ConsultationRecords { get; }
        IConsultationTypeRepository ConsultationTypes { get; }

        // ==================== Clinic Related Repositories ====================
        IClinicRepository Clinics { get; }
        IClinicPhoneNumberRepository ClinicPhoneNumbers { get; }
        IClinicPhotosRepository ClinicPhotos { get; }
        IClinicServiceRepository ClinicServices { get; }

        // ==================== Laboratory Related Repositories ====================
        ILaboratoryRepository Laboratories { get; }
        ILabOrderRepository LabOrders { get; }
        ILabPrescriptionRepository LabPrescriptions { get; }
        ILabPrescriptionItemRepository LabPrescriptionItems { get; }
        ILabResultRepository LabResults { get; }
        ILabServiceRepository LabServices { get; }
        ILabTestRepository LabTests { get; }
        ILabWorkingHoursRepository LabWorkingHours { get; }
        ILaboratoryDocumentRepository LaboratoryDocuments { get; }

        // ==================== Pharmacy Related Repositories ====================
        IPharmacyRepository Pharmacies { get; }
        IPharmacyOrderRepository PharmacyOrders { get; }
        IPrescriptionRepository Prescriptions { get; }
        IMedicationRepository Medications { get; }
        IPharmacyWorkingHoursRepository PharmacyWorkingHours { get; }
        IPrescribedMedicationRepository PrescribedMedications { get; }
        IDispensingRecordRepository DispensingRecords { get; }
        IPharmacyDocumentRepository PharmacyDocuments { get; }

        // ==================== Review Related Repositories ====================
        IDoctorReviewRepository DoctorReviews { get; }
        ILaboratoryReviewRepository LaboratoryReviews { get; }
        IPharmacyReviewRepository PharmacyReviews { get; }

        // ==================== Shared Repositories ====================
        IVerifierRepository Verifiers { get; }
        INotificationRepository Notifications { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        // ==================== Payment Related Repositories ====================
        IPaymentRepository Payments { get; }

        // ==================== Chat/AI Bot Repositories ====================
        IConversationRepository Conversations { get; }
        IConversationMessageRepository ConversationMessages { get; }

        // ==================== Generic Repository ====================
        IGenericRepository<T> Repository<T>() where T : class;

        // ==================== Transaction Methods ====================
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}