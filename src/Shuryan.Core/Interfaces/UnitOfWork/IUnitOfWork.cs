using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.Repositories.ClinicRepositories;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Core.Interfaces.Repositories.ReviewRepositories;
using Shuryan.Core.Interfaces.Repositories.MedicationRepositories;

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

        // ==================== Pharmacy Related Repositories ====================
        IPharmacyRepository Pharmacies { get; }
        IPharmacyOrderRepository PharmacyOrders { get; }
        IPrescriptionRepository Prescriptions { get; }
        IPharmacyDocumentRepository PharmacyDocuments { get; }

        // ==================== Laboratory Related Repositories ====================
        ILaboratoryRepository Laboratories { get; }
        ILabOrderRepository LabOrders { get; }
        ILabPrescriptionRepository LabPrescriptions { get; }
        ILabPrescriptionItemRepository LabPrescriptionItems { get; }
        ILabServiceRepository LabServices { get; }
        ILabTestRepository LabTests { get; }
        ILabResultRepository LabResults { get; }
        ILabWorkingHoursRepository LabWorkingHours { get; }
        ILaboratoryDocumentRepository LaboratoryDocuments { get; }

        // ==================== Medication Related Repositories ====================
        IMedicationRepository Medications { get; }
        IPrescribedMedicationRepository PrescribedMedications { get; }

        // ==================== Review Related Repositories ====================
        IDoctorReviewRepository DoctorReviews { get; }
        ILaboratoryReviewRepository LaboratoryReviews { get; }
        IPharmacyReviewRepository PharmacyReviews { get; }

        // ==================== Shared Repositories ====================
        IAddressRepository Addresses { get; }
        IVerifierRepository Verifiers { get; }
        INotificationRepository Notifications { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        // ==================== Transaction Methods ====================
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}