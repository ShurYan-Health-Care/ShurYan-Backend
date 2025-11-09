using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Laboratories
{
    public class LaboratoryDocumentRepository : GenericRepository<LaboratoryDocument>, ILaboratoryDocumentRepository
    {
        public LaboratoryDocumentRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<LaboratoryDocument>> GetByLaboratoryIdAsync(Guid laboratoryId)
        {
            return await _dbSet
                .Include(d => d.Laboratory)
                .Where(d => d.LaboratoryId == laboratoryId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LaboratoryDocument>> GetPendingDocumentsAsync()
        {
            return await _dbSet
                .Include(d => d.Laboratory)
                .Where(d => d.Status == VerificationDocumentStatus.UnderReview)
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LaboratoryDocument>> GetByStatusAsync(VerificationDocumentStatus status)
        {
            return await _dbSet
                .Include(d => d.Laboratory)
                .Where(d => d.Status == status)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
    }
}

