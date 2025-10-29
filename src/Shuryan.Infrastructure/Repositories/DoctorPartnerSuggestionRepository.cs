using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Partners;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories
{
    public class DoctorPartnerSuggestionRepository : GenericRepository<DoctorPartnerSuggestion>, IDoctorPartnerSuggestionRepository
    {
        public DoctorPartnerSuggestionRepository(ShuryanDbContext context) : base(context)
        {
        }

        public async Task<DoctorPartnerSuggestion?> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId);
        }

        public async Task<bool> DeleteByDoctorIdAsync(Guid doctorId)
        {
            var suggestion = await GetByDoctorIdAsync(doctorId);
            if (suggestion == null)
                return false;

            _dbSet.Remove(suggestion);
            return true;
        }
    }
}
