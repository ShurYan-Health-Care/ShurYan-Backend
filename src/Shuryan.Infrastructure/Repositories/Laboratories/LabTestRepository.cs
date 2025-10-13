using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Laboratories
{
    public class LabTestRepository : GenericRepository<LabTest>, ILabTestRepository
    {
        public LabTestRepository(ShuryanDbContext context) : base(context) { }

        public async Task<LabTest?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Code == code);
        }

        public async Task<IEnumerable<LabTest>> GetTestsByCategoryAsync(LabTestCategory category)
        {
            return await _dbSet
                .Where(t => t.Category == category)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabTest>> SearchTestsAsync(string searchTerm)
        {
            var lowerSearch = searchTerm.ToLower();
            return await _dbSet
                .Where(t => t.Name.ToLower().Contains(lowerSearch) 
                    || t.Code.ToLower().Contains(lowerSearch))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabTest>> GetMostRequestedTestsAsync(int count)
        {
            return await _dbSet
                .Include(t => t.PrescriptionItems)
                .OrderByDescending(t => t.PrescriptionItems.Count)
                .Take(count)
                .ToListAsync();
        }
    }
}

