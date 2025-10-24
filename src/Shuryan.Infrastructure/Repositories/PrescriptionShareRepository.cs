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
    public class PrescriptionShareRepository : GenericRepository<PrescriptionShare>, IPrescriptionShareRepository
    {
        public PrescriptionShareRepository(ShuryanDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PrescriptionShare>> GetByPharmacyIdAsync(Guid pharmacyId)
        {
            return await _context.PrescriptionShares
                .Include(s => s.Prescription)
                .Where(s => s.PharmacyId == pharmacyId)
                .OrderByDescending(s => s.SharedAt)
                .ToListAsync();
        }

        public async Task<PrescriptionShare?> GetByShareCodeAsync(string shareCode)
        {
            return await _context.PrescriptionShares
                .Include(s => s.Prescription)
                .FirstOrDefaultAsync(s => s.ShareCode == shareCode);
        }
    }
}
