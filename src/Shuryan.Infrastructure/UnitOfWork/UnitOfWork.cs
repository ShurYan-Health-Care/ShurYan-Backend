using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using Shuryan.Infrastructure.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Infrastructure.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork
    {
        private readonly ShuryanDbContext _context;
        private IDbContextTransaction? _transaction;

        // Lazy Initialization Fields - مش هنعمل Instance إلا لما نحتاجه
        private IDoctorRepository? _doctors;
        private IPatientRepository? _patients;
        private IVerifierRepository? _verifiers;
        private IAppointmentRepository? _appointments;
        private IConsultationRecordRepository? _consultationRecords;
        private IConsultationTypeRepository? _consultationTypes;
        private IDoctorConsultationRepository? _doctorConsultations;
        private IDoctorAvailabilityRepository? _doctorAvailabilities;
        private IDoctorOverrideRepository? _doctorOverrides;
        private IAddressRepository? _addresses;
        private IDoctorDocumentRepository? _doctorDocuments;
        private IMedicalHistoryItemRepository? _medicalHistoryItems;

        public UnitOfWork(ShuryanDbContext context)
        {
            _context = context;
        }

        // كل Property بيعمل Lazy Initialization للـ Repository المطلوب

        public IDoctorRepository Doctors
            => _doctors ??= new DoctorRepository(_context);

        public IPatientRepository Patients
            => _patients ??= new PatientRepository(_context);

        public IVerifierRepository Verifiers
            => _verifiers ??= new VerifierRepository(_context);
        public IAppointmentRepository Appointments
            => _appointments ??= new AppointmentRepository(_context);

        public IConsultationRecordRepository ConsultationRecords
            => _consultationRecords ??= new ConsultationRecordRepository(_context);

        public IConsultationTypeRepository ConsultationTypes
            => _consultationTypes ??= new ConsultationTypeRepository(_context);

        public IDoctorConsultationRepository DoctorConsultations
            => _doctorConsultations ??= new DoctorConsultationRepository(_context);

        public IDoctorAvailabilityRepository DoctorAvailabilities
            => _doctorAvailabilities ??= new DoctorAvailabilityRepository(_context);

        public IDoctorOverrideRepository DoctorOverrides
            => _doctorOverrides ??= new DoctorOverrideRepository(_context);
        public IAddressRepository Addresses
            => _addresses ??= new AddressRepository(_context);
        public IDoctorDocumentRepository DoctorDocuments
            => _doctorDocuments ??= new DoctorDocumentRepository(_context);

        public IMedicalHistoryItemRepository MedicalHistoryItems
            => _medicalHistoryItems ??= new MedicalHistoryItemRepository(_context);

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
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started. Call BeginTransactionAsync first.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started. Call BeginTransactionAsync first.");

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
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