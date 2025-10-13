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
    public class LabServiceRepository : GenericRepository<LabService>, ILabServiceRepository
    {
        public LabServiceRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<LabService>> GetLabServicesAsync(Guid laboratoryId)
        {
            return await _dbSet
                .Include(ls => ls.LabTest)
                .Where(ls => ls.LaboratoryId == laboratoryId)
                .OrderBy(ls => ls.LabTest.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabService>> GetAvailableLabServicesAsync(Guid laboratoryId)
        {
            return await _dbSet
                .Include(ls => ls.LabTest)
                .Where(ls => ls.LaboratoryId == laboratoryId && ls.IsAvailable)
                .OrderBy(ls => ls.LabTest.Name)
                .ToListAsync();
        }

        public async Task<LabService?> GetLabServiceByTestAsync(Guid laboratoryId, Guid labTestId)
        {
            return await _dbSet
                .Include(ls => ls.LabTest)
                .Include(ls => ls.Laboratory)
                .FirstOrDefaultAsync(ls => ls.LaboratoryId == laboratoryId 
                    && ls.LabTestId == labTestId);
        }

        public async Task<IEnumerable<LabService>> GetLaboratoriesOfferingTestAsync(Guid labTestId)
        {
            return await _dbSet
                .Include(ls => ls.Laboratory)
                    .ThenInclude(l => l.Address)
                .Include(ls => ls.LabTest)
                .Where(ls => ls.LabTestId == labTestId && ls.IsAvailable)
                .OrderBy(ls => ls.Price)
                .ToListAsync();
        }
    }
}

