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
    public class MedicalHistoryItemRepository : GenericRepository<MedicalHistoryItem>, IMedicalHistoryItemRepository
    {
        private readonly ShuryanDbContext _context;

        public MedicalHistoryItemRepository(ShuryanDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicalHistoryItem>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context.MedicalHistoryItems
                .Where(m => m.PatientId == patientId)
                .OrderBy(m => m.Type)
                .ThenByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicalHistoryItem>> GetByTypeAsync(Guid patientId, MedicalHistoryType type)
        {
            return await _context.MedicalHistoryItems
                .Where(m => m.PatientId == patientId &&
                           m.Type == type)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}