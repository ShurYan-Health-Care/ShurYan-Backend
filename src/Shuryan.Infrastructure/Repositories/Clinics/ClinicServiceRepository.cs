using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Interfaces.Repositories.ClinicRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Clinics
{
    public class ClinicServiceRepository : GenericRepository<ClinicService>, IClinicServiceRepository
    {
        public ClinicServiceRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<ClinicService>> GetClinicServicesAsync(Guid clinicId)
        {
            return await _dbSet
                .Where(cs => cs.ClinicId == clinicId)
                .OrderBy(cs => cs.CreatedAt)
                .ToListAsync();
        }
    }
}

