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
    public class MedicationRepository : GenericRepository<Medication>, IMedicationRepository
    {
        public MedicationRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Medication?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(m => m.Id.ToString() == code);
        }

        public async Task<IEnumerable<Medication>> SearchMedicationsAsync(string searchTerm)
        {
            var lowerSearch = searchTerm.ToLower();
            return await _dbSet
                .Where(m => m.BrandName.ToLower().Contains(lowerSearch) 
                    || (m.GenericName != null && m.GenericName.ToLower().Contains(lowerSearch)))
                .OrderBy(m => m.BrandName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetByManufacturerAsync(string manufacturer)
        {
            return await _dbSet
                .OrderBy(m => m.BrandName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetPrescriptionRequiredMedicationsAsync()
        {
            return await _dbSet
                .OrderBy(m => m.BrandName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetMostPrescribedMedicationsAsync(int count)
        {
            return await _dbSet
                .Include(m => m.PrescribedMedications)
                .OrderByDescending(m => m.PrescribedMedications.Count)
                .Take(count)
                .ToListAsync();
        }
    }
}

