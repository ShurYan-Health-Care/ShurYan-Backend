using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.Repositories.ChatRepositories;
using Shuryan.Core.Interfaces.Repositories.ClinicRepositories;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Core.Interfaces.Repositories.ReviewRepositories;
using Shuryan.Core.Interfaces.Repositories.MedicationRepositories;
using Shuryan.Infrastructure.Data;
using Shuryan.Infrastructure.Repositories.Doctors;
using Shuryan.Infrastructure.Repositories.Patients;
using Shuryan.Infrastructure.Repositories.Medical;
using Shuryan.Infrastructure.Repositories.Clinics;
using Shuryan.Infrastructure.Repositories.Pharmacies;
using Shuryan.Infrastructure.Repositories.Laboratories;
using Shuryan.Infrastructure.Repositories.Reviews;
using Shuryan.Infrastructure.Repositories.Medications;
using Shuryan.Infrastructure.Repositories.Shared;
using Shuryan.Infrastructure.Repositories.Chat;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Infrastructure.Repositories;

namespace Shuryan.Infrastructure.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork
    {
        private readonly ShuryanDbContext _context;
        private IDbContextTransaction? _transaction;

        // ==================== Doctor Related Fields ====================
        private IDoctorRepository? _doctors;
        private IDoctorAvailabilityRepository? _doctorAvailabilities;
        private IDoctorConsultationRepository? _doctorConsultations;
        private IDoctorOverrideRepository? _doctorOverrides;
        private IDoctorDocumentRepository? _doctorDocuments;
        private IDoctorPartnerSuggestionRepository? _doctorPartnerSuggestions;

        // ==================== Patient Related Fields ====================
        private IPatientRepository? _patients;
        private IMedicalHistoryItemRepository? _medicalHistoryItems;

        // ==================== Medical/Appointment Related Fields ====================
        private IAppointmentRepository? _appointments;
        private IConsultationRecordRepository? _consultationRecords;
        private IConsultationTypeRepository? _consultationTypes;

        // ==================== Clinic Related Fields ====================
        private IClinicRepository? _clinics;
        private IClinicPhoneNumberRepository? _clinicPhoneNumbers;
        private IClinicPhotosRepository? _clinicPhotos;
        private IClinicServiceRepository? _clinicServices;

        // ==================== Laboratory Related Fields ====================
        private ILaboratoryRepository? _laboratories;
        private ILabOrderRepository? _labOrders;
        private ILabPrescriptionRepository? _labPrescriptions;
        private ILabPrescriptionItemRepository? _labPrescriptionItems;
        private ILabResultRepository? _labResults;
        private ILabServiceRepository? _labServices;
        private ILabTestRepository? _labTests;
        private ILabWorkingHoursRepository? _labWorkingHours;
        private ILaboratoryDocumentRepository? _laboratoryDocuments;

        // ==================== Pharmacy Related Fields ====================
        private IPharmacyRepository? _pharmacies;
        private IPharmacyOrderRepository? _pharmacyOrders;
        private IPrescriptionRepository? _prescriptions;
        private IMedicationRepository? _medications;
        private IPharmacyWorkingHoursRepository? _pharmacyWorkingHours;
        private IPrescribedMedicationRepository? _prescribedMedications;
        private IDispensingRecordRepository? _dispensingRecords;
        private IPharmacyDocumentRepository? _pharmacyDocuments;

        // ==================== Review Related Fields ====================
        private IDoctorReviewRepository? _doctorReviews;
        private ILaboratoryReviewRepository? _laboratoryReviews;
        private IPharmacyReviewRepository? _pharmacyReviews;

        // ==================== Shared Fields ====================
        private IVerifierRepository? _verifiers;
        private INotificationRepository? _notifications;
        private IRefreshTokenRepository? _refreshTokens;

        // ==================== Payment Related Fields ====================
        private IPaymentRepository? _payments;

        // ==================== Chat/AI Bot Fields ====================
        private IConversationRepository? _conversations;
        private IConversationMessageRepository? _conversationMessages;


        public UnitOfWork(ShuryanDbContext context)
        {
            _context = context;
        }

        // ==================== Doctor Related Properties ====================
        public IDoctorRepository Doctors =>
            _doctors ??= new DoctorRepository(_context);

        public IDoctorAvailabilityRepository DoctorAvailabilities =>
            _doctorAvailabilities ??= new DoctorAvailabilityRepository(_context);

        public IDoctorConsultationRepository DoctorConsultations =>
            _doctorConsultations ??= new DoctorConsultationRepository(_context);

        public IDoctorOverrideRepository DoctorOverrides =>
            _doctorOverrides ??= new DoctorOverrideRepository(_context);

        public IDoctorDocumentRepository DoctorDocuments =>
            _doctorDocuments ??= new DoctorDocumentRepository(_context);

        public IDoctorPartnerSuggestionRepository DoctorPartnerSuggestions =>
            _doctorPartnerSuggestions ??= new DoctorPartnerSuggestionRepository(_context);

        // ==================== Patient Related Properties ====================
        public IPatientRepository Patients =>
            _patients ??= new PatientRepository(_context);

        public IMedicalHistoryItemRepository MedicalHistoryItems =>
            _medicalHistoryItems ??= new MedicalHistoryItemRepository(_context);

        // ==================== Medical/Appointment Related Properties ====================
        public IAppointmentRepository Appointments =>
            _appointments ??= new AppointmentRepository(_context);

        public IConsultationRecordRepository ConsultationRecords =>
            _consultationRecords ??= new ConsultationRecordRepository(_context);

        public IConsultationTypeRepository ConsultationTypes =>
            _consultationTypes ??= new ConsultationTypeRepository(_context);

        // ==================== Clinic Related Properties ====================
        public IClinicRepository Clinics =>
            _clinics ??= new ClinicRepository(_context);

        public IClinicPhoneNumberRepository ClinicPhoneNumbers =>
            _clinicPhoneNumbers ??= new ClinicPhoneNumberRepository(_context);

        public IClinicPhotosRepository ClinicPhotos =>
            _clinicPhotos ??= new ClinicPhotosRepository(_context);

        public IClinicServiceRepository ClinicServices =>
            _clinicServices ??= new ClinicServiceRepository(_context);

        public IPharmacyWorkingHoursRepository PharmacyWorkingHours
            => _pharmacyWorkingHours ??= new PharmacyWorkingHoursRepository(_context);

        // ==================== Laboratory Related Properties ====================
        public ILaboratoryRepository Laboratories =>
            _laboratories ??= new LaboratoryRepository(_context);

        public ILabOrderRepository LabOrders =>
            _labOrders ??= new LabOrderRepository(_context);

        public ILabPrescriptionRepository LabPrescriptions =>
            _labPrescriptions ??= new LabPrescriptionRepository(_context);

        public ILabPrescriptionItemRepository LabPrescriptionItems =>
            _labPrescriptionItems ??= new LabPrescriptionItemRepository(_context);

        public ILabResultRepository LabResults =>
            _labResults ??= new LabResultRepository(_context);

        public ILabServiceRepository LabServices =>
            _labServices ??= new LabServiceRepository(_context);

        public ILabTestRepository LabTests =>
            _labTests ??= new LabTestRepository(_context);

        public ILabWorkingHoursRepository LabWorkingHours =>
            _labWorkingHours ??= new LabWorkingHoursRepository(_context);

        public ILaboratoryDocumentRepository LaboratoryDocuments =>
            _laboratoryDocuments ??= new LaboratoryDocumentRepository(_context);

        // ==================== Pharmacy Related Properties ====================
        public IPharmacyRepository Pharmacies =>
            _pharmacies ??= new PharmacyRepository(_context);

        public IPharmacyOrderRepository PharmacyOrders =>
            _pharmacyOrders ??= new PharmacyOrderRepository(_context);

        public IPrescriptionRepository Prescriptions =>
            _prescriptions ??= new PrescriptionRepository(_context);

        public IMedicationRepository Medications =>
            _medications ??= new MedicationRepository(_context);

        public IPrescribedMedicationRepository PrescribedMedications =>
            _prescribedMedications ??= new PrescribedMedicationRepository(_context);

        public IDispensingRecordRepository DispensingRecords =>
            _dispensingRecords ??= new DispensingRecordRepository(_context);

        public IPharmacyDocumentRepository PharmacyDocuments =>
            _pharmacyDocuments ??= new PharmacyDocumentRepository(_context);

        // ==================== Review Related Properties ====================
        public IDoctorReviewRepository DoctorReviews =>
            _doctorReviews ??= new DoctorReviewRepository(_context);

        public ILaboratoryReviewRepository LaboratoryReviews =>
            _laboratoryReviews ??= new LaboratoryReviewRepository(_context);

        public IPharmacyReviewRepository PharmacyReviews =>
            _pharmacyReviews ??= new PharmacyReviewRepository(_context);

        // ==================== Shared Properties ====================
        public IVerifierRepository Verifiers =>
            _verifiers ??= new VerifierRepository(_context);

        public INotificationRepository Notifications =>
            _notifications ??= new NotificationRepository(_context);

        public IRefreshTokenRepository RefreshTokens =>
            _refreshTokens ??= new RefreshTokenRepository(_context);

        // ==================== Payment Related Properties ====================
        public IPaymentRepository Payments =>
            _payments ??= new Shuryan.Infrastructure.Repositories.Payments.PaymentRepository(_context);

        // ==================== Chat/AI Bot Properties ====================
        public IConversationRepository Conversations =>
            _conversations ??= new ConversationRepository(_context);

        public IConversationMessageRepository ConversationMessages =>
            _conversationMessages ??= new ConversationMessageRepository(_context);

        // ==================== Generic Repository ====================
        public IGenericRepository<T> Repository<T>() where T : class
        {
            return new GenericRepository<T>(_context);
        }

        // ==================== Transaction Methods ====================
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return _transaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                {
                await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
            }
        }

       // ==================== Dispose Pattern ====================

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}