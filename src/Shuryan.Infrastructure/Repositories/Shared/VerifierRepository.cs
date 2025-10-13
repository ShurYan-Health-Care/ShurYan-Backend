using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Shared
{
    public class VerifierRepository : GenericRepository<Verifier>, IVerifierRepository
    {
        public VerifierRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Verifier?> GetByIdWithVerifiedEntitiesAsync(Guid id)
        {
            return await _dbSet
                .Include(v => v.VerifiedDoctors)
                    .ThenInclude(d => d.Clinic)
                .Include(v => v.VerifiedLabors)
                    .ThenInclude(l => l.Address)
                .Include(v => v.VerifiedPharmacies)
                    .ThenInclude(p => p.Address)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<Verifier?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(v => v.Email == email && !v.IsDeleted);
        }

        public async Task<int> GetVerifiedDoctorsCountAsync(Guid verifierId)
        {
            return await _dbSet
                .Where(v => v.Id == verifierId && !v.IsDeleted)
                .SelectMany(v => v.VerifiedDoctors)
                .CountAsync();
        }
    }
}

