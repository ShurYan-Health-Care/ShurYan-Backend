using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Laboratories
{
    public class LabPrescriptionItemRepository : GenericRepository<LabPrescriptionItem>, ILabPrescriptionItemRepository
    {
        public LabPrescriptionItemRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<LabPrescriptionItem>> GetItemsByPrescriptionAsync(Guid labPrescriptionId)
        {
            return await _dbSet
                .Include(i => i.LabTest)
                .Where(i => i.LabPrescriptionId == labPrescriptionId)
                .OrderBy(i => i.LabTest.Name)
                .ToListAsync();
        }

        public async Task<LabPrescriptionItem?> GetItemAsync(Guid labPrescriptionId, Guid labTestId)
        {
            return await _dbSet
                .Include(i => i.LabTest)
                .FirstOrDefaultAsync(i => i.LabPrescriptionId == labPrescriptionId 
                    && i.LabTestId == labTestId);
        }
    }
}

