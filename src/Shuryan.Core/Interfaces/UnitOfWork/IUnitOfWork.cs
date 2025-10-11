using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Shuryan.Core.Interfaces.Repositories;

namespace Shuryan.Core.Interfaces.UnitOfWork
{
	public interface IUnitOfWork : IDisposable
    {
        // ==================== Repository Properties ====================
        IDoctorRepository Doctors { get; }
        IPatientRepository Patients { get; }
        IVerifierRepository Verifiers { get; }
        IAppointmentRepository Appointments { get; }
        IConsultationRecordRepository ConsultationRecords { get; }
        IConsultationTypeRepository ConsultationTypes { get; }
        IDoctorConsultationRepository DoctorConsultations { get; }
        IDoctorAvailabilityRepository DoctorAvailabilities { get; }
        IDoctorOverrideRepository DoctorOverrides { get; }
        IAddressRepository Addresses { get; }
        IDoctorDocumentRepository DoctorDocuments { get; }
        IMedicalHistoryItemRepository MedicalHistoryItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}