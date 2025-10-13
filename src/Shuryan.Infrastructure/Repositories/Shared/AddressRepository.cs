using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Shared
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        public AddressRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<Address>> GetByCityAsync(string city)
        {
            var normalizedCity = city.Trim().ToLower();

            return await _dbSet
                .Where(a => a.City.ToLower() == normalizedCity && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Address>> GetByGovernorateAsync(Governorate governorate)
        {
            return await _dbSet
                .Where(a => a.Governorate == governorate && !a.IsDeleted)
                .ToListAsync();
        }
    }
}

