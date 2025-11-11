using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Pharmacies
{
    public class PrescriptionRepository : GenericRepository<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Prescription?> GetPrescriptionWithDetailsAsync(Guid prescriptionId)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);
        }

        public async Task<IEnumerable<Prescription>> GetPagedPrescriptionsForPatientAsync(
            Guid patientId, 
            int pageNumber, 
            int pageSize)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Prescription>> GetAllPrescriptionsForPatientWithDetailsAsync(Guid patientId)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Appointment)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Prescription>> GetActivePrescriptionsForPatientAsync(Guid patientId)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
                .Where(p => p.PatientId == patientId  
                    && p.PharmacyOrder == null)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Prescription?> FindByPrescriptionNumberAsync(string prescriptionNumber)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync(p => p.PrescriptionNumber == prescriptionNumber);
        }

        public async Task<IEnumerable<Prescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.PrescribedMedications)
                .Where(p => p.CreatedAt >= startDate 
                    && p.CreatedAt <= endDate)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Prescription>> GetPrescriptionsContainingMedicationAsync(Guid medicationId)
        {
            return await _dbSet
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
                .Where(p => p.PrescribedMedications.Any(pm => pm.MedicationId == medicationId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}

