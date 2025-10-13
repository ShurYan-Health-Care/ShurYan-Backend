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
    public class ClinicPhotosRepository : GenericRepository<ClinicPhoto>, IClinicPhotosRepository
    {
        public ClinicPhotosRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<ClinicPhoto>> GetClinicPhotosAsync(Guid clinicId)
        {
            return await _dbSet
                .Where(cp => cp.ClinicId == clinicId)
                .OrderBy(cp => cp.CreatedAt)
                .ToListAsync();
        }
    }
}

