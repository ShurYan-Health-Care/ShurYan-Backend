using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Shared
{
    public class PharmacyDocumentRepository : GenericRepository<PharmacyDocument>, IPharmacyDocumentRepository
    {
        public PharmacyDocumentRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<PharmacyDocument>> GetByPharmacyIdAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Include(d => d.Pharmacy)
                .Where(d => d.PharmacyId == pharmacyId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PharmacyDocument>> GetPendingDocumentsAsync()
        {
            return await _dbSet
                .Include(d => d.Pharmacy)
                .Where(d => d.Status == VerificationDocumentStatus.UnderReview)
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PharmacyDocument>> GetByStatusAsync(VerificationDocumentStatus status)
        {
            return await _dbSet
                .Include(d => d.Pharmacy)
                .Where(d => d.Status == status)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
    }
}

