using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Interfaces.Repositories.MedicationRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Medications
{
    public class PrescribedMedicationRepository : GenericRepository<PrescribedMedication>, IPrescribedMedicationRepository
    {
        public PrescribedMedicationRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<PrescribedMedication>> GetByPrescriptionAsync(Guid prescriptionId)
        {
            return await _dbSet
                .Include(pm => pm.Medication)
                .Where(pm => pm.MedicationPrescriptionId == prescriptionId)
                .OrderBy(pm => pm.Medication.BrandName)
                .ToListAsync();
        }

        public async Task<PrescribedMedication?> GetPrescribedMedicationAsync(Guid prescriptionId, Guid medicationId)
        {
            return await _dbSet
                .Include(pm => pm.Medication)
                .FirstOrDefaultAsync(pm => pm.MedicationPrescriptionId == prescriptionId 
                    && pm.MedicationId == medicationId);
        }

        public async Task<IEnumerable<PrescribedMedication>> GetPrescriptionsContainingMedicationAsync(Guid medicationId)
        {
            return await _dbSet
                .Include(pm => pm.MedicationPrescription)
                    .ThenInclude(p => p.Doctor)
                .Include(pm => pm.Medication)
                .Where(pm => pm.MedicationId == medicationId)
                .OrderByDescending(pm => pm.MedicationPrescription.CreatedAt)
                .ToListAsync();
        }
    }
}

