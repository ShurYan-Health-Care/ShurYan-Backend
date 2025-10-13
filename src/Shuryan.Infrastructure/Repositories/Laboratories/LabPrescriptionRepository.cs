using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Laboratories
{
    public class LabPrescriptionRepository : GenericRepository<LabPrescription>, ILabPrescriptionRepository
    {
        public LabPrescriptionRepository(ShuryanDbContext context) : base(context) { }

        public async Task<LabPrescription?> GetPrescriptionWithDetailsAsync(Guid prescriptionId)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Appointment)
                .Include(p => p.Items)
                    .ThenInclude(i => i.LabTest)
                .Include(p => p.LabOrder)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);
        }

        public async Task<LabPrescription?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Items)
                    .ThenInclude(i => i.LabTest)
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
        }

        public async Task<IEnumerable<LabPrescription>> GetPagedPrescriptionsForPatientAsync(Guid patientId, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.LabTest)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabPrescription>> GetActivePrescriptionsForPatientAsync(Guid patientId)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.LabTest)
                .Where(p => p.PatientId == patientId 
                    && p.LabOrder == null)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabPrescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Items)
                .Where(p => p.CreatedAt >= startDate 
                    && p.CreatedAt <= endDate)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabPrescription>> GetPrescriptionsContainingTestAsync(Guid testId)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Items)
                    .ThenInclude(i => i.LabTest)
                .Where(p => p.Items.Any(i => i.LabTestId == testId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}

