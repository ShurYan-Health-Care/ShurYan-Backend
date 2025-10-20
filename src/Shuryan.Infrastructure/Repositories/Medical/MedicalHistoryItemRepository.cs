using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Medical
{
    public class MedicalHistoryItemRepository : GenericRepository<MedicalHistoryItem>, IMedicalHistoryItemRepository
    {
        public MedicalHistoryItemRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<MedicalHistoryItem>> GetByPatientIdAsync(Guid patientId)
        {
            return await _dbSet
                .Include(mhi => mhi.Patient)
                .Where(mhi => mhi.PatientId == patientId)
                .OrderByDescending(mhi => mhi.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicalHistoryItem>> GetByTypeAsync(Guid patientId, MedicalHistoryType type)
        {
            return await _dbSet
                .Where(mhi => mhi.PatientId == patientId && mhi.Type == type)
                .OrderByDescending(mhi => mhi.CreatedAt)
                .ToListAsync();
        }
    }
}

