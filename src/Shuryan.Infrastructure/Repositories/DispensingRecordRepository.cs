using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories
{
    public class DispensingRecordRepository : GenericRepository<DispensingRecord>, IDispensingRecordRepository
    {
        public DispensingRecordRepository(ShuryanDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DispensingRecord>> GetByPrescriptionIdAsync(Guid prescriptionId)
        {
            return await _context.DispensingRecords
                .Include(d => d.DispensedMedications)
                    .ThenInclude(m => m.Medication)
                .Where(d => d.PrescriptionId == prescriptionId)
                .OrderByDescending(d => d.DispensedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DispensingRecord>> GetByPharmacyIdAsync(Guid pharmacyId)
        {
            return await _context.DispensingRecords
                .Include(d => d.Prescription)
                .Include(d => d.DispensedMedications)
                .Where(d => d.PharmacyId == pharmacyId)
                .OrderByDescending(d => d.DispensedAt)
                .ToListAsync();
        }
    }
}
