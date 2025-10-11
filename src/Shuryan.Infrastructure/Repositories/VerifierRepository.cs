using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories
{
    public class VerifierRepository : GenericRepository<Verifier>, IVerifierRepository
    {
        private readonly ShuryanDbContext _context;

        public VerifierRepository(ShuryanDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Verifier?> GetByIdWithVerifiedEntitiesAsync(Guid id)
        {
            return await _context.Verifiers
                .Include(v => v.VerifiedDoctors)
                .Include(v => v.VerifiedLabors)
                .Include(v => v.VerifiedPharmacies)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<Verifier?> GetByEmailAsync(string email)
        {
            return await _context.Verifiers
               .FirstOrDefaultAsync(v => v.Email.ToLower() == email.ToLower() && !v.IsDeleted);
        }

        public async Task<int> GetVerifiedDoctorsCountAsync(Guid verifierId)
        {
            return await _context.Doctors
                .Where(d => d.VerifierId == verifierId && 
                           d.VerificationStatus == VerificationStatus.Verified &&
                           !d.IsDeleted)
                .CountAsync();
        }
    }
}