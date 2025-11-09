using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Doctors
{
    public class DoctorDocumentRepository : GenericRepository<DoctorDocument>, IDoctorDocumentRepository
    {
        public DoctorDocumentRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<DoctorDocument>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(dd => dd.Doctor)
                .Where(dd => dd.DoctorId == doctorId)
                .OrderByDescending(dd => dd.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorDocument>> GetPendingDocumentsAsync()
        {
            return await _dbSet
                .Include(dd => dd.Doctor)
                .Where(dd => dd.Status == VerificationDocumentStatus.UnderReview)
                .OrderBy(dd => dd.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorDocument>> GetByStatusAsync(VerificationDocumentStatus status)
        {
            return await _dbSet
                .Include(dd => dd.Doctor)
                .Where(dd => dd.Status == status)
                .OrderByDescending(dd => dd.CreatedAt)
                .ToListAsync();
        }
    }
}

