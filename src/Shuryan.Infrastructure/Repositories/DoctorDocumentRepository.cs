using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories
{
    public class DoctorDocumentRepository : GenericRepository<DoctorDocument>, IDoctorDocumentRepository
    {
        private readonly ShuryanDbContext _context;

        public DoctorDocumentRepository(ShuryanDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorDocument>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _context.DoctorDocument
                .Where(d => d.DoctorId == doctorId)
                .OrderBy(d => d.Type)
                .ThenByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorDocument>> GetByStatusAsync(VerificationDocumentStatus status)
        {
            return await _context.DoctorDocument
                .Include(d => d.Doctor)
                .Where(d => d.Status == status)
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorDocument>> GetPendingDocumentsAsync()
        {
            return await GetByStatusAsync(VerificationDocumentStatus.Pending);
        }
    }
}